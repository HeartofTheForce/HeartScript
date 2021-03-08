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

        public static OperatorInfo OperatorInfo(Keyword keyword, uint leftPrecedence, uint rightPrecedence)
        {
            return new OperatorInfo(
                keyword,
                leftPrecedence,
                rightPrecedence,
                1,
                null,
                null,
                (token, leftNode, rightNodes) => new BinaryNode(token.Keyword, leftNode!, rightNodes[0]));
        }
    }
}
