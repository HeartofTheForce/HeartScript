using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class IdentifierNode : INode
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class IdentifierNodeBuilder : NodeBuilder
    {
        public IdentifierNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (operand != null)
                throw new System.ArgumentException($"{nameof(operand)}");

            acknowledgeToken = false;

            return new IdentifierNode(current.Value);
        }
    }
}
