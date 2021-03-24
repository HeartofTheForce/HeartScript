using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class TernaryNode : INode
    {
        public Token Token { get; }
        public INode Target { get; }
        public INode OptionA { get; }
        public INode OptionB { get; }

        public TernaryNode(
            Token token,
            INode target,
            INode optionA,
            INode optionB)
        {
            Token = token;
            Target = target;
            OptionA = optionA;
            OptionB = optionB;
        }

        public override string ToString()
        {
            return $"{Token.Value} {OptionA} {OptionB}";
        }

        public static OperatorInfo OperatorInfo(uint leftPrecedence, uint rightPrecedence)
        {
            return new OperatorInfo(
                Keyword.Ternary,
                leftPrecedence,
                rightPrecedence,
                2,
                Keyword.Colon,
                null,
                (token, leftNode, rightNodes) => new TernaryNode(token, leftNode!, rightNodes[0], rightNodes[1]));
        }
    }
}
