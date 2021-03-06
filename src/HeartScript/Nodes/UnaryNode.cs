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

    public class UnaryNodeBuilder : NodeBuilder, INodeBuilder
    {
        public UnaryNodeBuilder(OperatorInfo operatorInfo, Token token, INode leftNode) : base(operatorInfo, token, leftNode)
        {
        }

        public bool IsComplete() => RightNodes.Count() == 1;

        public INode Build()
        {
            return new UnaryNode(Token.Keyword, RightNodes.Single());
        }
    }
}
