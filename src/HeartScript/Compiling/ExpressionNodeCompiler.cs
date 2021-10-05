using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HeartScript.Expressions;
#pragma warning disable IDE0019

namespace HeartScript.Compiling
{
    public static class ExpressionNodeCompiler
    {
        const int Nullary = 0;
        const int Prefix = 1;
        const int Postfix = 2;
        const int Infix = 3;

        private readonly static Func<ExpressionNode, Expression?>[][] s_nodeCompilers;

        static ExpressionNodeCompiler()
        {
            s_nodeCompilers = new Func<ExpressionNode, Expression?>[4][];

            s_nodeCompilers[Nullary] = new Func<ExpressionNode, Expression?>[]
            {
                ParseInt,
                ParseDouble,
                CompileRoundBracket,
                ParseIdentifier,
            };

            s_nodeCompilers[Prefix] = new Func<ExpressionNode, Expression?>[]
            {
                CompilePrefix,
            };

            s_nodeCompilers[Postfix] = new Func<ExpressionNode, Expression?>[]
            {
                CompilePostfix,
                CompileStaticCall(typeof(Math)),
            };

            s_nodeCompilers[Infix] = new Func<ExpressionNode, Expression?>[]
            {
                CompileBinary,
                CompileTernary,
            };
        }

        static int GetFixity(ExpressionNode node)
        {
            int index = 0;

            if (node.HaveRight)
                index |= 1 << 0;

            if (node.HaveLeft)
                index |= 1 << 1;

            return index;
        }

        public static Func<T> CompileFunction<T>(ExpressionNode node)
        {
            var compiledExpression = Compile(node);
            compiledExpression = ConvertIfRequired(compiledExpression, typeof(T));

            return Expression.Lambda<Func<T>>(compiledExpression).Compile();
        }

        static Expression Compile(ExpressionNode node)
        {
            var fixityCompilers = s_nodeCompilers[GetFixity(node)];

            Expression? compiled = null;
            foreach (var compiler in fixityCompilers)
            {
                compiled = compiler(node);
                if (compiled != null)
                    break;
            }

            if (compiled == null)
                throw new ArgumentException($"{nameof(node)} does not have a matching compiler");

            return compiled;
        }

        static Expression? ParseDouble(ExpressionNode node)
        {
            if (double.TryParse(node.Value, out double value))
                return Expression.Constant(value);

            return null;
        }

        static Expression? ParseInt(ExpressionNode node)
        {
            if (int.TryParse(node.Value, out int value))
                return Expression.Constant(value);

            return null;
        }

        static Expression? ParseIdentifier(ExpressionNode node)
        {
            return Expression.Constant(node.Value);
        }

        static Expression? CompileRoundBracket(ExpressionNode node)
        {
            if (node.Value == "(")
            {
                var mid = Compile((ExpressionNode)node.Children[0]);
                return mid;
            }

            return null;
        }

        static Expression? CompilePrefix(ExpressionNode node)
        {
            var right = Compile((ExpressionNode)node.Children[^1]);

            switch (node.Value)
            {
                case "+": return Expression.UnaryPlus(right);
                case "-": return Expression.Negate(right);
                case "~": return Expression.Not(right);
                default:
                    break;
            }

            return null;
        }

        static Expression? CompilePostfix(ExpressionNode node)
        {
            var left = Compile((ExpressionNode)node.Children[0]);

            switch (node.Value)
            {
                case "!": return left;
                default:
                    break;
            }

            return null;
        }

        static Expression CompileCall(ExpressionNode callNode, Expression? instance, Type type, BindingFlags bindingFlags)
        {
            var left = callNode.Children[0];

            if (left.Children?.Count > 0)
                throw new Exception($"{nameof(left)}.{nameof(left.Children)} must be empty");

            string methodName = left.Value;
            if (methodName == null)
                throw new Exception($"{nameof(methodName)} cannot be null");

            int parameterCount = callNode.Children.Count - 1;
            var parameters = new Expression[parameterCount];
            var parameterTypes = new Type[parameterCount];

            for (int i = 0; i < parameterCount; i++)
            {
                parameters[i] = Compile((ExpressionNode)callNode.Children[i + 1]);
                parameterTypes[i] = parameters[i].Type;
            }

            var methodInfo = type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
            if (methodInfo == null)
                throw new Exception($"{type.FullName} does not have an overload matching '{methodName}({string.Join(',', parameterTypes.Select(x => x.Name))})'");

            var expectedParameters = methodInfo.GetParameters();
            for (int i = 0; i < parameterCount; i++)
            {
                parameters[i] = ConvertIfRequired(parameters[i], expectedParameters[i].ParameterType);
            }

            return Expression.Call(instance, methodInfo, parameters);
        }

        static Func<ExpressionNode, Expression?> CompileStaticCall(Type type)
        {
            return (node) =>
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
                if (node.Value == "(")
                    return CompileCall(node, null, type, bindingFlags);

                return null;
            };
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

        static Expression? CompileBinary(ExpressionNode node)
        {
            var left = Compile((ExpressionNode)node.Children[0]);
            var right = Compile((ExpressionNode)node.Children[^1]);

            if (IsFloatingPoint(left.Type) && IsIntegral(right.Type))
                right = Expression.Convert(right, left.Type);
            else if (IsIntegral(left.Type) && IsFloatingPoint(right.Type))
                left = Expression.Convert(left, right.Type);

            switch (node.Value)
            {
                case "*": return Expression.Multiply(left, right);
                case "/": return Expression.Divide(left, right);
                case "+": return Expression.Add(left, right);
                case "-": return Expression.Subtract(left, right);
                case "<=": return Expression.LessThanOrEqual(left, right);
                case ">=": return Expression.GreaterThanOrEqual(left, right);
                case "<": return Expression.LessThan(left, right);
                case ">": return Expression.GreaterThan(left, right);
                case "&": return Expression.And(left, right);
                case "^": return Expression.ExclusiveOr(left, right);
                case "|": return Expression.Or(left, right);
                default:
                    break;
            }

            return null;
        }

        static Expression? CompileTernary(ExpressionNode node)
        {
            if (node.Value == "?")
            {
                var left = Compile((ExpressionNode)node.Children[0]);
                var mid = Compile((ExpressionNode)node.Children[1]);
                var right = Compile((ExpressionNode)node.Children[2]);

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
    }
}
