using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class BracketNodeBuilder : NodeBuilder
    {
        private readonly Keyword _closingKeyword;

        public BracketNodeBuilder(OperatorInfo operatorInfo, Keyword closingKeyword) : base(operatorInfo)
        {
            _closingKeyword = closingKeyword;
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (current.Keyword == OperatorInfo.Keyword && operand == null)
            {
                acknowledgeToken = false;
                return null;
            }

            if (operand == null)
                throw new ExpressionTermException(current);

            if (current.Keyword != _closingKeyword)
                throw new UnexpectedTokenException(current, _closingKeyword);
            else
            {
                acknowledgeToken = true;
                return operand;
            }
        }
    }
}
