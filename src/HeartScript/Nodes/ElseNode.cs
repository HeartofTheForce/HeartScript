using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class ElseNode : INode
    {
        public Token Token { get; }
        public INode IfStatement { get; }
        public INode ElseStatement { get; }

        public ElseNode(Token token, INode ifStatement, INode elseStatement)
        {
            Token = token;
            IfStatement = ifStatement;
            ElseStatement = elseStatement;
        }

        public override string ToString()
        {
            return $"({Token.Value} {IfStatement} {ElseStatement})";
        }

        public static OperatorInfo OperatorInfo()
        {
            return new OperatorInfo(
                Keyword.Else,
                uint.MaxValue - 1,
                uint.MaxValue,
                1,
                null,
                null,
                (token, leftNode, rightNodes) => new ElseNode(token, leftNode!, rightNodes[0]));
        }
    }
}
