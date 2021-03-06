using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class BinaryNode : INode
    {
        public Keyword Keyword { get; }
        public INode Left { get; }
        public INode Right { get; }

        public BinaryNode(Keyword keyword, INode left, INode right)
        {
            Keyword = keyword;
            Left = left;
            Right = right;
        }
    }

    public class BinaryNodeBuilder : NodeBuilder, INodeBuilder
    {
        public BinaryNodeBuilder(OperatorInfo operatorInfo, Token token, INode leftNode) : base(operatorInfo, token, leftNode)
        {
        }

        public bool IsComplete() => RightNodes.Count() == 1;

        public INode Build()
        {
            return new BinaryNode(Token.Keyword, LeftNode, RightNodes.Single());
        }
    }
}
