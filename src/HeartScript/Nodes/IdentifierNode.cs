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

        public static OperatorInfo OperatorInfo()
        {
            return new OperatorInfo(
                Keyword.Identifier,
                null,
                0,
                0,
                null,
                null,
                (token, leftNode, rightNodes) => new ConstantNode(token.Value));
        }
    }
}
