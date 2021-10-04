using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Expressions
{
    public class ExpressionPattern : IPattern
    {
        private readonly IEnumerable<OperatorInfo> _patterns;

        public ExpressionPattern(IEnumerable<OperatorInfo> patterns)
        {
            _patterns = patterns;
        }

        public static INode Parse(IEnumerable<OperatorInfo> patterns, ParserContext ctx)
        {
            var expressionPattern = new ExpressionPattern(patterns);

            var parser = new PatternParser();
            parser.Patterns["expr"] = expressionPattern;
            var result = parser.TryMatch(expressionPattern, ctx);

            if (result == null)
            {
                if (ctx.Exception != null)
                    throw ctx.Exception;
                else
                    throw new ArgumentException(nameof(ctx.Exception));
            }

            if (!ctx.IsEOF)
                throw new UnexpectedTokenException(ctx.Offset, "EOF");

            return result;
        }

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var nodeBuilders = new Stack<ExpressionNodeBuilder>();
            INode? operand = null;

            while (true)
            {
                var right = TryGetNodeBuilder(operand == null, parser, ctx);
                if (right == null)
                {
                    if (ctx.Exception != null && ctx.Offset < ctx.Exception.CharIndex)
                        return null;

                    while (nodeBuilders.Count > 0)
                    {
                        if (operand == null)
                        {
                            ctx.Exception = new ExpressionTermException(ctx.Offset);
                            return null;
                        }

                        operand = nodeBuilders.Pop().FeedOperandRight(operand);
                    }

                    if (operand != null)
                        return operand;

                    ctx.Exception = new ExpressionTermException(localOffset);
                    return null;
                }

                while (nodeBuilders.TryPeek(out var left) && left.IsEvaluatedBefore(right))
                {
                    if (operand == null)
                    {
                        ctx.Exception = new ExpressionTermException(ctx.Offset);
                        return null;
                    }

                    operand = nodeBuilders.Pop().FeedOperandRight(operand);
                }

                operand = right.FeedOperandLeft(operand);
                if (operand == null)
                    nodeBuilders.Push(right);
            }
        }

        private ExpressionNodeBuilder? TryGetNodeBuilder(bool wantOperand, PatternParser parser, ParserContext ctx)
        {
            foreach (var x in _patterns)
            {
                bool valid;
                if (wantOperand)
                    valid = x.IsPrefix() || x.IsNullary();
                else
                    valid = x.IsInfix() || x.IsPostfix();

                if (!valid)
                    continue;

                var result = parser.TryMatch(x.Pattern, ctx);
                if (result != null)
                    return new ExpressionNodeBuilder(x, result);
            }

            return null;
        }
    }

    public class ExpressionTermException : PatternException
    {
        public ExpressionTermException(int charIndex) : base(charIndex, $"Invalid Expression Term @ {charIndex}")
        {
        }
    }
}
