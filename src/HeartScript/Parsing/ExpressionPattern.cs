using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class ExpressionPattern : IPattern
    {
        private readonly IEnumerable<OperatorPattern> _patterns;

        public ExpressionPattern(IEnumerable<OperatorPattern> patterns)
        {
            _patterns = patterns;
        }

        public static INode Parse(IEnumerable<OperatorPattern> patterns, ParserContext ctx)
        {
            var expressionPattern = new ExpressionPattern(patterns);

            var parser = new Parser();
            parser.Patterns["expr"] = expressionPattern;
            var result = parser.TryMatch(expressionPattern, ctx);

            if (result.Value == null)
                throw new Exception(result.ErrorMessage);

            if (!ctx.IsEOF)
                throw new UnexpectedTokenException(ctx.Offset, "EOF");

            return result.Value;
        }

        public PatternResult Match(Parser parser, ParserContext ctx)
        {
            int startIndex = ctx.Offset;

            var nodeBuilders = new Stack<NodeBuilder>();
            INode? operand = null;

            while (true)
            {
                var nodeBuilder = TryGetNodeBuilder(operand != null, parser, ctx);
                if (nodeBuilder == null)
                {
                    while (nodeBuilders.Count > 0)
                    {
                        if (!TryReduce(ref operand, nodeBuilders))
                            throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                    }

                    return PatternResult.Success(startIndex, operand);
                }

                while (nodeBuilders.TryPeek(out var left) && left.IsEvaluatedBefore(nodeBuilder))
                {
                    if (!TryReduce(ref operand, nodeBuilders))
                        throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                }

                operand = nodeBuilder.FeedOperandLeft(operand);
                if (operand == null)
                    nodeBuilders.Push(nodeBuilder);
            }
        }

        private NodeBuilder? TryGetNodeBuilder(bool haveOperand, Parser parser, ParserContext ctx)
        {
            OperatorPattern? op = null;
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
                if (result.Value != null)
                {
                    op = x;
                    midNode = result.Value;
                    break;
                }
            }

            if (op == null || midNode == null)
                return null;

            return new NodeBuilder(op, midNode);
        }

        private bool TryReduce(ref INode? operand, Stack<NodeBuilder> nodeBuilders)
        {
            var nodeBuilder = nodeBuilders.Pop();
            operand = nodeBuilder.FeedOperandRight(operand);

            if (operand == null)
            {
                nodeBuilders.Push(nodeBuilder);
                return false;
            }

            return true;
        }
    }

    public class NodeBuilder
    {
        private readonly OperatorPattern _operatorPattern;
        private readonly INode _midNode;
        private INode? _leftNode;
        private INode? _rightNode;

        public NodeBuilder(OperatorPattern operatorPattern, INode midNode)
        {
            _operatorPattern = operatorPattern;
            _midNode = midNode;
        }

        public bool IsEvaluatedBefore(NodeBuilder right)
        {
            // if (_operatorPattern.RightPrecedence == null || right.LeftPrecedence == null)
            //     return _operatorPattern.RightPrecedence == null;
            if (_operatorPattern.RightPrecedence == null)
                return true;

            return _operatorPattern.RightPrecedence <= right._operatorPattern.LeftPrecedence;
        }

        public INode? FeedOperandLeft(INode? leftNode)
        {
            _leftNode = leftNode;
            return TryCompleteNode();
        }

        public INode? FeedOperandRight(INode? rightNode)
        {
            _rightNode = rightNode;
            return TryCompleteNode();
        }

        private INode? TryCompleteNode()
        {
            bool haveLeft = _leftNode != null;
            bool expectLeft = _operatorPattern.LeftPrecedence != null;
            if (haveLeft != expectLeft)
                return null;

            bool haveRight = _rightNode != null;
            bool expectRight = _operatorPattern.RightPrecedence != null;
            if (haveRight != expectRight)
                return null;

            return new ExpressionNode(_leftNode, _midNode, _rightNode);
        }
    }
}
