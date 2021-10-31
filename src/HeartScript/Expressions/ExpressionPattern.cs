using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Expressions
{
    public class ExpressionPattern : IPattern
    {
        private readonly IEnumerable<OperatorInfo> _operators;

        public ExpressionPattern(IEnumerable<OperatorInfo> operators)
        {
            _operators = operators;
        }

        public static IParseNode Parse(IEnumerable<OperatorInfo> operators, ParserContext ctx)
        {
            var expressionPattern = new ExpressionPattern(operators);

            var parser = new PatternParser();
            parser.Patterns["expr"] = expressionPattern;
            var result = expressionPattern.TryMatch(parser, ctx);

            if (result == null)
            {
                if (ctx.Exception != null)
                    throw ctx.Exception;
                else
                    throw new ArgumentException(nameof(ctx.Exception));
            }

            if (!ctx.IsEOF)
            {
                if (ctx.Exception != null && ctx.Exception.CharIndex > ctx.Offset)
                    throw ctx.Exception;
                else
                    throw new UnexpectedTokenException(ctx.Offset, "EOF");
            }

            return result;
        }

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            var nodeBuilders = new Stack<ExpressionNodeBuilder>();
            ExpressionNode? operand = null;

            while (true)
            {
                var right = TryGetNodeBuilder(operand == null, parser, ctx);
                if (right == null)
                {
                    if (operand == null)
                    {
                        if (ctx.Exception == null || ctx.Exception.CharIndex <= ctx.Offset)
                            ctx.Exception = new ExpressionTermException(ctx.Offset);

                        return null;
                    }

                    while (nodeBuilders.Count > 0)
                    {
                        operand = nodeBuilders.Pop().FeedOperandRight(operand);
                    }

                    return operand;
                }

                while (nodeBuilders.TryPeek(out var left) && IsEvaluatedBefore(left.OperatorInfo, right.OperatorInfo))
                {
                    operand = nodeBuilders.Pop().FeedOperandRight(operand);
                }

                operand = right.FeedOperandLeft(operand);
                if (operand == null)
                    nodeBuilders.Push(right);
            }
        }

        private ExpressionNodeBuilder? TryGetNodeBuilder(bool wantOperand, PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;
            foreach (var op in _operators)
            {
                bool valid;
                if (wantOperand)
                {
                    //Nullary, Prefix
                    valid = op.LeftPrecedence == null;
                }
                else
                {
                    //Postfix, Infix
                    valid = op.LeftPrecedence != null;
                }

                if (!valid)
                    continue;

                var result = op.Pattern.TryMatch(parser, ctx);
                if (result != null)
                {
                    if (localOffset == ctx.Offset)
                        throw new ZeroLengthMatchException(ctx.Offset);

                    return new ExpressionNodeBuilder(op, result);
                }
            }

            return null;
        }

        private static bool IsEvaluatedBefore(OperatorInfo left, OperatorInfo right)
        {
            if (left.RightPrecedence == null || right.LeftPrecedence == null)
                return left.RightPrecedence == null;

            return left.RightPrecedence <= right.LeftPrecedence;
        }
    }

    public class ExpressionTermException : PatternException
    {
        public ExpressionTermException(int charIndex) : base(charIndex, $"Invalid Expression Term @ {charIndex}")
        {
        }
    }
}
