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

            if (result.Exception != null)
                throw result.Exception;

            if (!ctx.IsEOF)
                throw new UnexpectedTokenException(ctx.Offset, "EOF");

            return result.Node;
        }

        public PatternResult Match(PatternParser parser, ParserContext ctx)
        {
            int startIndex = ctx.Offset;

            var nodeBuilders = new Stack<NodeBuilder>();
            INode? operand = null;

            while (true)
            {
                var nodeBuilder = TryGetNodeBuilder(operand != null, parser, ctx, out var furthestException);
                if (nodeBuilder == null)
                {
                    if (furthestException != null && ctx.Offset < furthestException.CharIndex)
                        return PatternResult.Error(furthestException);

                    while (nodeBuilders.Count > 0)
                    {
                        if (operand == null)
                            return PatternResult.Error(new ExpressionTermException(ctx.Offset));

                        operand = nodeBuilders.Pop().FeedOperandRight(operand);
                    }

                    if (operand != null)
                        return PatternResult.Success(operand);
                    else
                        return PatternResult.Error(new ExpressionTermException(startIndex));
                }

                while (nodeBuilders.TryPeek(out var left) && left.IsEvaluatedBefore(nodeBuilder))
                {
                    if (operand == null)
                        return PatternResult.Error(new ExpressionTermException(ctx.Offset));

                    operand = nodeBuilders.Pop().FeedOperandRight(operand);
                }

                operand = nodeBuilder.FeedOperandLeft(operand);
                if (operand == null)
                    nodeBuilders.Push(nodeBuilder);
            }
        }

        private NodeBuilder? TryGetNodeBuilder(bool haveOperand, PatternParser parser, ParserContext ctx, out PatternException? furthestException)
        {
            furthestException = null;

            OperatorInfo? op = null;
            INode? midNode = null;
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
                if (result.Node != null)
                {
                    op = x;
                    midNode = result.Node;
                    break;
                }

                if (result.Exception != null)
                {
                    if (furthestException == null || result.Exception.CharIndex > furthestException.CharIndex)
                        furthestException = result.Exception;
                }
            }

            if (op == null || midNode == null)
                return null;

            return new NodeBuilder(op, midNode);
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
            // if (_operatorInfo.RightPrecedence == null || right.LeftPrecedence == null)
            //     return _operatorInfo.RightPrecedence == null;
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
