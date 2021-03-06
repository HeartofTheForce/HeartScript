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
            if (operand == null)
                throw new System.ArgumentException($"{nameof(operand)}");

            acknowledgeToken = false;

            return new UnaryNode(OperatorInfo.Keyword, operand);
        }
    }
}
