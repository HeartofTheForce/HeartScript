using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class ConstantNode : INode
    {
        public Token Token { get; }

        public ConstantNode(Token token)
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
                new LexerPattern("\\d+(?:\\.\\d+)?", true),
                null,
                0,
                0,
                null,
                null,
                (token, leftNode, rightNodes) => new ConstantNode(token));
        }
    }
}
