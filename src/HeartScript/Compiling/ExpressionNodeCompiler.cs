using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HeartScript.Expressions;

namespace HeartScript.Compiling
{
    public static class ExpressionNodeCompiler
    {
        private delegate Expression CompileExpression(CompilerScope scope, ExpressionNode node);

        private static readonly Dictionary<string, CompileExpression> s_nodeCompilers = new Dictionary<string, CompileExpression>()
        {
            ["()"] = (scope, node) => Compile(scope, (ExpressionNode)node.MidNode.Children[1].Children[0]),
            ["$"] = CompileStaticCall(typeof(Math)),
            ["u+"] = (scope, node) => Expression.UnaryPlus(Compile(scope, node.RightNode)),
            ["u-"] = (scope, node) => Expression.Negate(Compile(scope, node.RightNode)),
            ["~"] = (scope, node) => Expression.Not(Compile(scope, node.RightNode)),
            ["!"] = (scope, node) => Compile(scope, node.LeftNode),
            ["*"] = CompileBinary(Expression.Multiply),
            ["/"] = CompileBinary(Expression.Divide),
            ["+"] = CompileBinary(Expression.Add),
            ["-"] = CompileBinary(Expression.Subtract),
            ["<="] = CompileBinary(Expression.LessThanOrEqual),
            [">="] = CompileBinary(Expression.GreaterThanOrEqual),
            ["<"] = CompileBinary(Expression.LessThan),
            [">"] = CompileBinary(Expression.GreaterThan),
            ["&"] = CompileBinary(Expression.And),
            ["^"] = CompileBinary(Expression.ExclusiveOr),
            ["|"] = CompileBinary(Expression.Or),
            ["?:"] = CompileTernary,
            ["Constant"] = ParseConstant,
            ["Identifier"] = ParseIdentifier,
        };

        public static Func<T> CompileFunction<T>(ExpressionNode node)
        {
            var scope = CompilerScope.Empty();

            var compiledExpression = Compile(scope, node);
            compiledExpression = ConvertIfRequired(compiledExpression, typeof(T));

            return Expression.Lambda<Func<T>>(compiledExpression).Compile();
        }

        public static Func<TContext, TResult> CompileFunction<TContext, TResult>(ExpressionNode node)
        {
            var parameters = new ParameterExpression[] { Expression.Parameter(typeof(TContext)) };
            var scope = CompilerScope.FromMembers(parameters[0]);

            var compiledExpression = Compile(scope, node);
            compiledExpression = ConvertIfRequired(compiledExpression, typeof(TResult));

            return Expression.Lambda<Func<TContext, TResult>>(compiledExpression, parameters).Compile();
        }

        static Expression Compile(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name != null && s_nodeCompilers.TryGetValue(node.Name, out var compiler))
                return compiler(scope, node);

            throw new ArgumentException($"{node.Name} does not have a matching compiler");
        }

        static Expression ParseConstant(CompilerScope scope, ExpressionNode node)
        {
            if (double.TryParse(node.MidNode.Value, out double doubleValue))
                return Expression.Constant(doubleValue);

            if (int.TryParse(node.MidNode.Value, out int intValue))
                return Expression.Constant(intValue);

            throw new ArgumentException(nameof(node));
        }

        static Expression ParseIdentifier(CompilerScope scope, ExpressionNode node)
        {
            if (scope.TryGetVariable(node.MidNode.Value, out var variable))
                return variable;

            throw new ArgumentException(nameof(node));
        }

        static Expression CompileCall(CompilerScope scope, ExpressionNode callNode, Expression? instance, Type type, BindingFlags bindingFlags)
        {
            var left = callNode.LeftNode;

            if (left.Name != "Identifier")
                throw new Exception($"{nameof(left)}.{nameof(left.Name)} is not Identifier");

            string methodName = left.MidNode.Value;
            if (methodName == null)
                throw new Exception($"{nameof(methodName)} cannot be null");

            var parameterNodes = CompilerHelper.GetChildren<ExpressionNode>(callNode.MidNode);
            var parameters = new Expression[parameterNodes.Count];
            var parameterTypes = new Type[parameterNodes.Count];

            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = Compile(scope, parameterNodes[i]);
                parameterTypes[i] = parameters[i].Type;
            }

            var methodInfo = type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
            if (methodInfo == null)
                throw new Exception($"{type.FullName} does not have an overload matching '{methodName}({string.Join(',', parameterTypes.Select(x => x.Name))})'");

            var expectedParameters = methodInfo.GetParameters();
            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = ConvertIfRequired(parameters[i], expectedParameters[i].ParameterType);
            }

            return Expression.Call(instance, methodInfo, parameters);
        }

        static CompileExpression CompileStaticCall(Type type)
        {
            return (scope, node) =>
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
                return CompileCall(scope, node, null, type, bindingFlags);
            };
        }

        static CompileExpression CompileBinary(Func<Expression, Expression, Expression> compiler)
        {
            return (scope, node) =>
            {
                var left = Compile(scope, node.LeftNode);
                var right = Compile(scope, node.RightNode);

                if (IsFloatingPoint(left.Type) && IsIntegral(right.Type))
                    right = Expression.Convert(right, left.Type);
                else if (IsIntegral(left.Type) && IsFloatingPoint(right.Type))
                    left = Expression.Convert(left, right.Type);

                return compiler(left, right);
            };
        }

        static Expression CompileTernary(CompilerScope scope, ExpressionNode node)
        {
            var left = Compile(scope, node.LeftNode);
            var mid = Compile(scope, (ExpressionNode)node.MidNode.Children[1].Children[0]);
            var right = Compile(scope, node.RightNode);

            if (IsIntegral(mid.Type) && IsFloatingPoint(right.Type))
                mid = Expression.Convert(mid, right.Type);
            else if (IsFloatingPoint(mid.Type) && IsIntegral(right.Type))
                right = Expression.Convert(right, mid.Type);

            return Expression.Condition(left, mid, right);
        }

        static Expression ConvertIfRequired(Expression expression, Type expectedType)
        {
            if (expression.Type != expectedType)
                return Expression.Convert(expression, expectedType);

            return expression;
        }

        static bool IsFloatingPoint(Type type) =>
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal);

        static bool IsIntegral(Type type) =>
            type == typeof(sbyte) ||
            type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong);
    }
}
