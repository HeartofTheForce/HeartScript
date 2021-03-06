using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class ConstantNode : INode
    {
        public string Value { get; }

        public ConstantNode(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class ConstantNodeBuilder : NodeBuilder
    {
        public ConstantNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (operand != null)
                throw new System.ArgumentException($"{nameof(operand)}");

            acknowledgeToken = true;
            return new ConstantNode(current.Value);
        }
    }
}
