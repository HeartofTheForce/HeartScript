using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public interface INode
    {
    }

    public class ErrorNode : INode
    {
        public OperatorInfo? OperatorInfo { get; }
        public Token Token { get; }
        public string Message { get; }

        private ErrorNode(OperatorInfo operatorInfo, Token token, string message)
        {
            OperatorInfo = operatorInfo;
            Token = token;
            Message = message;
        }

        private ErrorNode(Token token, string message)
        {
            Token = token;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Token}, {Message}";
        }

        public static ErrorNode UnexpectedToken(OperatorInfo operatorInfo, Token token, Keyword expected)
        {
            return new ErrorNode(operatorInfo, token, $"Expected '{expected}'");
        }

        public static ErrorNode UnexpectedToken(Token token)
        {
            return new ErrorNode(token, $"Unexpected '{token.Keyword}'");
        }

        public static ErrorNode InvalidExpressionTerm(OperatorInfo operatorInfo, Token token)
        {
            return new ErrorNode(operatorInfo, token, $"Invalid expression term");
        }
    }
}
