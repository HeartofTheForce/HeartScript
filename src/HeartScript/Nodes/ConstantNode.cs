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

    public class ConstantNodeBuilder : NodeBuilder, INodeBuilder
    {
        public ConstantNodeBuilder(OperatorInfo operatorInfo, Token token, INode leftNode) : base(operatorInfo, token, leftNode)
        {
        }

        public bool IsComplete() => true;

        public INode Build()
        {
            return new ConstantNode(Token.Value);
        }
    }
}
