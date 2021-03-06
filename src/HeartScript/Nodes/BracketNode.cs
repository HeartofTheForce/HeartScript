using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class BracketNode : INode
    {
        public Keyword Keyword { get; }
        public INode Target { get; }

        public BracketNode(Keyword keyword, INode target)
        {
            Keyword = keyword;
            Target = target;
        }
    }

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

            acknowledgeToken = true;

            if (operand == null)
                return ErrorNode.InvalidExpressionTerm(current.CharOffset, current.Keyword);

            if (current.Keyword != _closingKeyword)
                return ErrorNode.UnexpectedToken(current.CharOffset, _closingKeyword);
            else
                return operand;
        }
    }
}
