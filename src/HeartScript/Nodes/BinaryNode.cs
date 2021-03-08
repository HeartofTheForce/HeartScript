using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class BinaryNode : INode
    {
        public Token Token { get; }
        public INode Left { get; }
        public INode Right { get; }

        public BinaryNode(Token token, INode left, INode right)
        {
            Token = token;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return $"({Token.Value} {Left} {Right})";
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
                (token, leftNode, rightNodes) => new BinaryNode(token, leftNode!, rightNodes[0]));
        }
    }
}
