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

        public static OperatorInfo OperatorInfo()
        {
            return new OperatorInfo(
                Keyword.Constant,
                null,
                0,
                0,
                null,
                null,
                (token, leftNode, rightNodes) => new ConstantNode(token.Value));
        }
    }
}
