using System;
using System.Linq;
using System.Linq.Expressions;
using HeartScript.Expressions;

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
                ParseFloat,
                ParseInt,
                CompileRoundBracket,
            };

            s_nodeCompilers[Prefix] = new Func<ExpressionNode, Expression?>[]
            {
                CompilePrefix,
            };

            s_nodeCompilers[Postfix] = new Func<ExpressionNode, Expression?>[]
            {
                CompilePostfix,
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

        public static Expression Compile(ExpressionNode node)
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

        static Expression? ParseFloat(ExpressionNode node)
        {
            if (float.TryParse(node.Value, out float value))
                return Expression.Constant(value);

            return null;
        }

        static Expression? ParseInt(ExpressionNode node)
        {
            if (int.TryParse(node.Value, out int value))
                return Expression.Constant(value);

            return null;
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

        static Expression? CompileBinary(ExpressionNode node)
        {
            var left = Compile((ExpressionNode)node.Children[0]);
            var right = Compile((ExpressionNode)node.Children[^1]);

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
    }
}
