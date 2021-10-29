using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HeartScript.Expressions;
using HeartScript.Parsing;
using HeartScript.Peg.Patterns;

namespace HeartScript.Compiling
{
    public static class ExpressionNodeCompiler
    {
        private delegate Expression CompileExpression(CompilerScope scope, ExpressionNode node);

        private static readonly Dictionary<string, CompileExpression> s_nodeCompilers = new Dictionary<string, CompileExpression>()
        {
            ["()"] = CompileRoundBracket,
            ["$"] = CompileStaticCall(typeof(Math)),
            ["u+"] = (scope, node) =>
            {
                if (node.RightNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");

                return Expression.UnaryPlus(Compile(scope, node.RightNode));
            },
            ["u-"] = (scope, node) =>
            {
                if (node.RightNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");

                return Expression.Negate(Compile(scope, node.RightNode));
            },
            ["~"] = (scope, node) =>
            {
                if (node.RightNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");

                return Expression.Not(Compile(scope, node.RightNode));
            },
            ["!"] = (scope, node) =>
            {
                if (node.LeftNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");

                return Compile(scope, node.LeftNode);
            },
            ["*"] = CompileBinary(Expression.Multiply),
            ["/"] = CompileBinary(Expression.Divide),
            ["+"] = CompileBinary(Expression.Add),
            ["-"] = CompileBinary(Expression.Subtract),
            ["<="] = CompileBinary(Expression.LessThanOrEqual),
            [">="] = CompileBinary(Expression.GreaterThanOrEqual),
            ["<"] = CompileBinary(Expression.LessThan),
            [">"] = CompileBinary(Expression.GreaterThan),
            ["=="] = CompileBinary(Expression.Equal),
            ["!="] = CompileBinary(Expression.NotEqual),
            ["&"] = CompileBinary(Expression.And),
            ["^"] = CompileBinary(Expression.ExclusiveOr),
            ["|"] = CompileBinary(Expression.Or),
            ["?:"] = CompileTernary,
            ["real"] = ParseReal,
            ["integral"] = ParseIntegral,
            ["boolean"] = ParseBoolean,
            ["identifier"] = ParseIdentifier,
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
            if (node.Key != null && s_nodeCompilers.TryGetValue(node.Key, out var compiler))
                return compiler(scope, node);

            throw new ArgumentException($"{node.Key} does not have a matching compiler");
        }

        static Expression CompileRoundBracket(CompilerScope scope, ExpressionNode node)
        {
            var sequenceNode = (SequenceNode)node.MidNode;
            var lookupNode = (LookupNode)sequenceNode.Children[1];
            return Compile(scope, (ExpressionNode)lookupNode.Node);
        }

        static Expression ParseReal(CompilerScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (double.TryParse(valueNode.Value, out double value))
                return Expression.Constant(value);

            throw new ArgumentException(nameof(node));
        }

        static Expression ParseIntegral(CompilerScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (int.TryParse(valueNode.Value, out int value))
                return Expression.Constant(value);

            throw new ArgumentException(nameof(node));
        }

        static Expression ParseBoolean(CompilerScope scope, ExpressionNode node)
        {
            var choiceNode = (ChoiceNode)node.MidNode;
            var valueNode = (ValueNode)choiceNode.Node;
            if (bool.TryParse(valueNode.Value, out bool value))
                return Expression.Constant(value);

            throw new ArgumentException(nameof(node));
        }

        static Expression ParseIdentifier(CompilerScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (scope.TryGetVariable(valueNode.Value, out var variable))
                return variable;

            throw new ArgumentException(nameof(node));
        }

        static Expression CompileCall(CompilerScope scope, ExpressionNode callNode, Expression? instance, Type type, BindingFlags bindingFlags)
        {
            if (callNode.LeftNode == null)
                throw new Exception($"{nameof(callNode.LeftNode)} cannot be null");

            if (callNode.LeftNode.Key != "identifier")
                throw new Exception($"{nameof(callNode.LeftNode)} is not identifier");

            var leftValueNode = (ValueNode)callNode.LeftNode.MidNode;
            string methodName = leftValueNode.Value;
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
                if (node.LeftNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");
                if (node.RightNode == null)
                    throw new Exception($"{nameof(node.RightNode)} cannot be null");

                var left = Compile(scope, node.LeftNode);
                var right = Compile(scope, node.RightNode);

                if (IsReal(left.Type) && IsIntegral(right.Type))
                    right = Expression.Convert(right, left.Type);
                else if (IsIntegral(left.Type) && IsReal(right.Type))
                    left = Expression.Convert(left, right.Type);

                return compiler(left, right);
            };
        }

        static Expression CompileTernary(CompilerScope scope, ExpressionNode node)
        {
            if (node.LeftNode == null)
                throw new Exception($"{nameof(node.LeftNode)} cannot be null");
            if (node.RightNode == null)
                throw new Exception($"{nameof(node.RightNode)} cannot be null");

            var left = Compile(scope, node.LeftNode);
            var right = Compile(scope, node.RightNode);

            var sequenceNode = (SequenceNode)node.MidNode;
            var lookupNode = (LookupNode)sequenceNode.Children[1];
            var mid = Compile(scope, (ExpressionNode)lookupNode.Node);

            if (IsIntegral(mid.Type) && IsReal(right.Type))
                mid = Expression.Convert(mid, right.Type);
            else if (IsReal(mid.Type) && IsIntegral(right.Type))
                right = Expression.Convert(right, mid.Type);

            return Expression.Condition(left, mid, right);
        }

        static Expression ConvertIfRequired(Expression expression, Type expectedType)
        {
            if (expression.Type != expectedType)
                return Expression.Convert(expression, expectedType);

            return expression;
        }

        static bool IsReal(Type type) =>
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
