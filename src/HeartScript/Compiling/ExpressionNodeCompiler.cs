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
        private delegate Expression? CompileExpression(CompilerScope scope, ExpressionNode node);

        const int Nullary = 0;
        const int Prefix = 1;
        const int Postfix = 2;
        const int Infix = 3;

        private readonly static CompileExpression[][] s_nodeCompilers;

        static ExpressionNodeCompiler()
        {
            s_nodeCompilers = new CompileExpression[4][];

            s_nodeCompilers[Nullary] = new CompileExpression[]
            {
                ParseInt,
                ParseDouble,
                CompileRoundBracket,
                ParseIdentifier,
            };

            s_nodeCompilers[Prefix] = new CompileExpression[]
            {
                CompileUnaryPrefix,
            };

            s_nodeCompilers[Postfix] = new CompileExpression[]
            {
                CompileUnaryPostfix,
                CompileStaticCall(typeof(Math)),
            };

            s_nodeCompilers[Infix] = new CompileExpression[]
            {
                CompileBinary,
                CompileTernary,
            };
        }

        static int GetFixity(ExpressionNode node)
        {
            int index = 0;

            if (node.RightIndex != -1)
                index |= 1 << 0;

            if (node.LeftIndex != -1)
                index |= 1 << 1;

            return index;
        }

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
            var fixityCompilers = s_nodeCompilers[GetFixity(node)];

            Expression? compiled = null;
            foreach (var compiler in fixityCompilers)
            {
                compiled = compiler(scope, node);
                if (compiled != null)
                    break;
            }

            if (compiled == null)
                throw new ArgumentException($"{nameof(node)} does not have a matching compiler");

            return compiled;
        }

        static Expression? ParseDouble(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name == "Constant")
            {
                if (double.TryParse(node.MidNode.Value, out double value))
                    return Expression.Constant(value);
            }

            return null;
        }

        static Expression? ParseInt(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name == "Constant")
            {
                if (int.TryParse(node.MidNode.Value, out int value))
                    return Expression.Constant(value);
            }

            return null;
        }

        static Expression? ParseIdentifier(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name == "Identifier")
            {
                if (scope.TryGetVariable(node.MidNode.Value, out var variable))
                    return variable;
            }

            return null;
        }

        static Expression? CompileRoundBracket(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name == "()")
            {
                var mid = Compile(scope, (ExpressionNode)node.MidNode.Children[1].Children[0]);
                return mid;
            }

            return null;
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
                if (node.Name == "$")
                    return CompileCall(scope, node, null, type, bindingFlags);

                return null;
            };
        }

        private static readonly Dictionary<string, Func<Expression, Expression>> s_unaryPrefixCompilers = new Dictionary<string, Func<Expression, Expression>>()
        {
            ["+"] = Expression.UnaryPlus,
            ["-"] = Expression.Negate,
            ["~"] = Expression.Not,
        };

        static Expression? CompileUnaryPrefix(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name != null && s_unaryPrefixCompilers.TryGetValue(node.Name, out var compiler))
            {
                var right = Compile(scope, node.RightNode);
                return compiler(right);
            }

            return null;
        }

        private static readonly Dictionary<string, Func<Expression, Expression>> s_unaryPostfixCompilers = new Dictionary<string, Func<Expression, Expression>>()
        {
            ["!"] = (expression) => expression,
        };

        static Expression? CompileUnaryPostfix(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name != null && s_unaryPostfixCompilers.TryGetValue(node.Name, out var compiler))
            {
                var left = Compile(scope, node.LeftNode);
                return compiler(left);
            }

            return null;
        }

        private static readonly Dictionary<string, Func<Expression, Expression, Expression>> s_binaryCompilers = new Dictionary<string, Func<Expression, Expression, Expression>>()
        {
            ["*"] = Expression.Multiply,
            ["/"] = Expression.Divide,
            ["+"] = Expression.Add,
            ["-"] = Expression.Subtract,
            ["<="] = Expression.LessThanOrEqual,
            [">="] = Expression.GreaterThanOrEqual,
            ["<"] = Expression.LessThan,
            [">"] = Expression.GreaterThan,
            ["&"] = Expression.And,
            ["^"] = Expression.ExclusiveOr,
            ["|"] = Expression.Or,
        };

        static Expression? CompileBinary(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name != null && s_binaryCompilers.TryGetValue(node.Name, out var compiler))
            {
                var left = Compile(scope, node.LeftNode);
                var right = Compile(scope, node.RightNode);

                if (IsFloatingPoint(left.Type) && IsIntegral(right.Type))
                    right = Expression.Convert(right, left.Type);
                else if (IsIntegral(left.Type) && IsFloatingPoint(right.Type))
                    left = Expression.Convert(left, right.Type);

                return compiler(left, right);
            }

            return null;
        }

        static Expression? CompileTernary(CompilerScope scope, ExpressionNode node)
        {
            if (node.Name == "?:")
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

            return null;
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
