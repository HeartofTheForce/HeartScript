using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class UnaryNode : INode
    {
        public Keyword Keyword { get; }
        public INode Target { get; }

        public UnaryNode(Keyword keyword, INode target)
        {
            Keyword = keyword;
            Target = target;
        }
    }

    public class UnaryNodeBuilder : NodeBuilder
    {
        public UnaryNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (current.Keyword == OperatorInfo.Keyword && operand == null)
            {
                acknowledgeToken = false;
                return null;
            }

            if (operand == null)
            {
                acknowledgeToken = false;
                return ErrorNode.InvalidExpressionTerm(this, current);
            }

            acknowledgeToken = false;
            return new UnaryNode(OperatorInfo.Keyword, operand);
        }
    }
}
