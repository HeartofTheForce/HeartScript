using System.Collections.Generic;
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

        public override string ToString()
        {
            return $"{{{Keyword} {Left} {Right}}}";
        }

        public static NodeBuilder Builder(OperatorInfo operatorInfo)
        {
            return new NodeBuilder(
                operatorInfo,
                1,
                null,
                null,
                (token, leftNode, rightNodes) => new BinaryNode(token.Keyword, leftNode!, rightNodes[0]));
        }
    }
}
