using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public interface INode
    {
    }

    public class ErrorNode : INode
    {
        public OperatorInfo OperatorInfo { get; }
        public Token Token { get; }
        public string Message { get; }

        private ErrorNode(OperatorInfo operatorInfo, Token token, string message)
        {
            OperatorInfo = operatorInfo;
            Token = token;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Token}, {Message}";
        }

        public static ErrorNode UnexpectedToken(NodeBuilder nodeBuilder, Token token, Keyword expected)
        {
            return new ErrorNode(nodeBuilder.OperatorInfo, token, $"Expected '{expected}'");
        }

        public static ErrorNode InvalidExpressionTerm(NodeBuilder nodeBuilder, Token token)
        {
            return new ErrorNode(nodeBuilder.OperatorInfo, token, $"Invalid expression term");
        }
    }
}
