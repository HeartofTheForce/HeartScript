using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public interface INode
    {
    }

    public class ErrorNode : INode
    {
        public int CharOffset { get; }
        public string Message { get; }

        private ErrorNode(int charOffset, string message)
        {
            CharOffset = charOffset;
            Message = message;
        }

        public static ErrorNode UnexpectedToken(int charOffset, Keyword expected)
        {
            return new ErrorNode(charOffset, $"Expected '{expected}'");
        }

        public static ErrorNode InvalidExpressionTerm(int charOffset, Keyword unexpected)
        {
            return new ErrorNode(charOffset, $"Invalid expression term '{unexpected}'");
        }
    }
}
