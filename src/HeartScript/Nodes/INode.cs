using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public interface INode
    {
    }

    public class ErrorNode : INode
    {
        public OperatorInfo OperatorInfo { get; }
        public int CharOffset { get; }
        public string Message { get; }

        private ErrorNode(OperatorInfo operatorInfo, int charOffset, string message)
        {
            OperatorInfo = operatorInfo;
            CharOffset = charOffset;
            Message = message;
        }

        public static ErrorNode UnexpectedToken(NodeBuilder nodeBuilder, int charOffset, Keyword expected)
        {
            return new ErrorNode(nodeBuilder.OperatorInfo, charOffset, $"Expected '{expected}'");
        }

        public static ErrorNode InvalidExpressionTerm(NodeBuilder nodeBuilder, int charOffset, Keyword invalid)
        {
            return new ErrorNode(nodeBuilder.OperatorInfo, charOffset, $"Invalid expression term '{invalid}'");
        }
    }
}
