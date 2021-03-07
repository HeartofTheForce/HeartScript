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

        public static NodeBuilder Builder(OperatorInfo operatorInfo)
        {
            return new NodeBuilder(
                operatorInfo,
                0,
                null,
                null,
                (token, leftNode, rightNodes) => new ConstantNode(token.Value));
        }
    }
}
