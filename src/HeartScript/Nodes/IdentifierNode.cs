using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class IdentifierNode : INode
    {
        public Token Token { get; }

        public IdentifierNode(Token token)
        {
            Token = token;
        }

        public override string ToString()
        {
            return Token.Value;
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
                (token, leftNode, rightNodes) => new IdentifierNode(token));
        }
    }
}
