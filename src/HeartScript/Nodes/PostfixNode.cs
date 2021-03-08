using HeartScript.Parsing;

namespace HeartScript.Nodes
{

    public class PostfixNode : INode
    {
        public Token Token { get; }
        public INode Target { get; }

        public PostfixNode(Token token, INode target)
        {
            Token = token;
            Target = target;
        }

        public override string ToString()
        {
            return $"({Token.Value} {Target})";
        }

        public static OperatorInfo OperatorInfo(Keyword keyword, uint leftPrecedence)
        {
            return new OperatorInfo(
                keyword,
                leftPrecedence,
                0,
                0,
                null,
                null,
                (token, leftNode, rightNodes) => new PostfixNode(token, leftNode!));
        }
    }
}
