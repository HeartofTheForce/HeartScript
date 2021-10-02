using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class ParserContext
    {
        public Dictionary<string, IPattern> Patterns { get; }

        public ParserContext()
        {
            Patterns = new Dictionary<string, IPattern>();
        }

        public PatternResult TryMatch(IPattern pattern, Lexer lexer)
        {
            int startIndex = lexer.Offset;

            var result = pattern.Match(this, lexer);
            if (result.ErrorMessage != null)
                lexer.Offset = startIndex;

            return result;
        }
    }

    public interface IPattern
    {
        PatternResult Match(ParserContext ctx, Lexer lexer);
    }

    public class PatternResult
    {
        public int CharIndex { get; private set; }
        public INode? Value { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static PatternResult Success(int charIndex, INode value)
        {
            return new PatternResult()
            {
                CharIndex = charIndex,
                Value = value,
            };
        }

        public static PatternResult Error(int charIndex, string message)
        {
            return new PatternResult()
            {
                CharIndex = charIndex,
                ErrorMessage = message,
            };
        }
    }
}
