using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
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
                throw ctx.Exception;

            if (!ctx.IsEOF)
                throw new UnexpectedTokenException(ctx.Offset, "EOF");

            return result;
        }

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var nodeBuilders = new Stack<NodeBuilder>();
            INode? operand = null;

            while (true)
            {
                var nodeBuilder = TryGetNodeBuilder(operand != null, parser, ctx);
                if (nodeBuilder == null)
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

                while (nodeBuilders.TryPeek(out var left) && left.IsEvaluatedBefore(nodeBuilder))
                {
                    if (operand == null)
                    {
                        ctx.Exception = new ExpressionTermException(ctx.Offset);
                        return null;
                    }

                    operand = nodeBuilders.Pop().FeedOperandRight(operand);
                }

                operand = nodeBuilder.FeedOperandLeft(operand);
                if (operand == null)
                    nodeBuilders.Push(nodeBuilder);
            }
        }

        private NodeBuilder? TryGetNodeBuilder(bool haveOperand, PatternParser parser, ParserContext ctx)
        {
            foreach (var x in _patterns)
            {
                bool valid;
                if (haveOperand)
                    valid = x.IsInfix() || x.IsPostfix();
                else
                    valid = x.IsPrefix() || x.IsNullary();

                if (!valid)
                    continue;

                var result = parser.TryMatch(x.Pattern, ctx);
                if (result != null)
                    return new NodeBuilder(x, result);
            }

            return null;

        }
    }

    public class NodeBuilder
    {
        private readonly OperatorInfo _operatorInfo;
        private readonly INode _midNode;
        private INode? _leftNode;
        private INode? _rightNode;

        public NodeBuilder(OperatorInfo operatorInfo, INode midNode)
        {
            _operatorInfo = operatorInfo;
            _midNode = midNode;
        }

        public bool IsEvaluatedBefore(NodeBuilder right)
        {
            if (_operatorInfo.RightPrecedence == null)
                return true;

            return _operatorInfo.RightPrecedence <= right._operatorInfo.LeftPrecedence;
        }

        public INode? FeedOperandLeft(INode? leftNode)
        {
            _leftNode = leftNode;
            return TryCompleteNode();
        }

        public INode FeedOperandRight(INode rightNode)
        {
            _rightNode = rightNode;
            return TryCompleteNode() ?? throw new Exception($"{nameof(NodeBuilder)} is incomplete");
        }

        private INode? TryCompleteNode()
        {
            bool haveLeft = _leftNode != null;
            bool expectLeft = _operatorInfo.LeftPrecedence != null;
            if (haveLeft != expectLeft)
                return null;

            bool haveRight = _rightNode != null;
            bool expectRight = _operatorInfo.RightPrecedence != null;
            if (haveRight != expectRight)
                return null;

            return new ExpressionNode(_leftNode, _midNode, _rightNode);
        }
    }
}
