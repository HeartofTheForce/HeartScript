using System;
using HeartScript.Parsing;

namespace HeartScript.Expressions
{
    public class ExpressionNodeBuilder
    {
        private readonly OperatorInfo _operatorInfo;
        private readonly INode _midNode;
        private ExpressionNode? _leftNode;
        private ExpressionNode? _rightNode;

        public ExpressionNodeBuilder(OperatorInfo operatorInfo, INode midNode)
        {
            _operatorInfo = operatorInfo;
            _midNode = midNode;
        }

        public bool IsEvaluatedBefore(ExpressionNodeBuilder right)
        {
            return OperatorInfo.IsEvaluatedBefore(_operatorInfo, right._operatorInfo);
        }

        public ExpressionNode? FeedOperandLeft(ExpressionNode? leftNode)
        {
            _leftNode = leftNode;
            return TryCompleteNode();
        }

        public ExpressionNode FeedOperandRight(ExpressionNode? rightNode)
        {
            _rightNode = rightNode;
            return TryCompleteNode() ?? throw new Exception($"{nameof(ExpressionNodeBuilder)} is incomplete");
        }

        private ExpressionNode? TryCompleteNode()
        {
            bool haveLeft = _leftNode != null;
            bool expectLeft = _operatorInfo.LeftPrecedence != null;
            if (haveLeft != expectLeft)
                return null;

            bool haveRight = _rightNode != null;
            bool expectRight = _operatorInfo.RightPrecedence != null;
            if (haveRight != expectRight)
                return null;

            return new ExpressionNode(_operatorInfo.Name, _leftNode, _midNode, _rightNode);
        }
    }
}
