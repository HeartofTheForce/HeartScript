using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class PrefixNode : INode
    {
        public Token Token { get; }
        public INode Target { get; }

        public PrefixNode(Token token, INode target)
        {
            Token = token;
            Target = target;
        }

        public override string ToString()
        {
            return $"({Token.Value} {Target})";
        }

        public static OperatorInfo OperatorInfo(Keyword keyword, uint rightPrecedence)
        {
            return new OperatorInfo(
                keyword,
                null,
                rightPrecedence,
                1,
                null,
                null,
                (token, leftNode, rightNodes) => new PrefixNode(token, rightNodes[0]));
        }
    }
}
