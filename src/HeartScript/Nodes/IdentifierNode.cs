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

    public class IdentifierNodeBuilder : NodeBuilder, INodeBuilder
    {
        public IdentifierNodeBuilder(OperatorInfo operatorInfo, Token token, INode leftNode) : base(operatorInfo, token, leftNode)
        {
        }

        public bool IsComplete() => true;

        public INode Build()
        {
            return new IdentifierNode(Token.Value);
        }
    }
}
