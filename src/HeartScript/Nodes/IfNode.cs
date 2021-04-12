using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class IfNode : INode
    {
        public Token Token { get; }
        public INode Condition { get; }
        public INode Statement { get; }

        public IfNode(Token token, INode condition, INode statement)
        {
            Token = token;
            Condition = condition;
            Statement = statement;
        }

        public override string ToString()
        {
            return $"({Token.Value} {Condition} {Statement})";
        }

        public static OperatorInfo OperatorInfo()
        {
            return new OperatorInfo(
                Keyword.If,
                null,
                uint.MaxValue,
                2,
                null,
                null,
                (token, leftNode, rightNodes) => new IfNode(token, rightNodes[0], rightNodes[1]));
        }
    }
}
