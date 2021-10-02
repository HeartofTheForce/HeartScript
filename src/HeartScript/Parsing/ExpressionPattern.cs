using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    class ExpressionNodeBuilder
    {
        private readonly OperatorPattern _operatorPattern;
        private INode? _left;
        private INode? _right;

        public ExpressionNodeBuilder(OperatorPattern operatorPattern)
        {
            _operatorPattern = operatorPattern;
        }


        public bool IsEvaluatedBefore(OperatorPattern right)
        {
            // if (_operatorPattern.RightPrecedence == null || right.LeftPrecedence == null)
            //     return _operatorPattern.RightPrecedence == null;
            if (_operatorPattern.RightPrecedence == null)
                return true;

            return _operatorPattern.RightPrecedence <= right.LeftPrecedence;
        }

        public INode? FeedOperandLeft(INode? left)
        {
            _left = left;
            return TryCompleteNode();
        }

        public INode? FeedOperandRight(INode? right)
        {
            _right = right;
            return TryCompleteNode();
        }

        private INode? TryCompleteNode()
        {
            bool haveLeft = _left != null;
            bool expectLeft = _operatorPattern.LeftPrecedence != null;
            if (haveLeft != expectLeft)
                return null;

            bool haveRight = _right != null;
            bool expectRight = _operatorPattern.RightPrecedence != null;
            if (haveRight != expectRight)
                return null;

            return _operatorPattern.BuildNode(_left, _right);
        }
    }
    public class ExpressionPattern : IPattern
    {
        private readonly IEnumerable<OperatorPattern> _patterns;

        public PatternResult Match(Parser parser, ParserContext ctx)
        {
            int startIndex = ctx.Offset;

            var nodeBuilders = new Stack<ExpressionNodeBuilder>();
            INode? operand = null;

            while (true)
            {
                var op = TryGetOperator(operand == null, parser, ctx);
                if (op == null)
                {
                    while (nodeBuilders.Count > 0)
                    {
                        if (!TryReduce(ref operand, nodeBuilders))
                            throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                    }

                    return PatternResult.Success(startIndex, operand);
                }

                while (nodeBuilders.TryPeek(out var left) && left.IsEvaluatedBefore(op))
                {
                    if (!TryReduce(ref operand, nodeBuilders))
                        throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                }

                var nodeBuilder = new ExpressionNodeBuilder(op);
                operand = nodeBuilder.FeedOperandLeft(operand);
                if (operand == null)
                    nodeBuilders.Push(nodeBuilder);
            }
        }

        private OperatorPattern? TryGetOperator(bool haveOperand, Parser parser, ParserContext ctx)
        {
            OperatorPattern op = null;
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
                    break;
                }
            }

            return op;
        }

        private bool TryReduce(ref INode? operand, Stack<ExpressionNodeBuilder> nodeBuilders)
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
}
