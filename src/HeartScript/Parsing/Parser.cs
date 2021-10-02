using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class Parser
    {
        public Dictionary<string, IPattern> Patterns { get; }

        public Parser()
        {
            Patterns = new Dictionary<string, IPattern>();
        }

        public PatternResult TryMatch(IPattern pattern, ParserContext ctx)
        {
            int startIndex = ctx.Offset;

            var result = pattern.Match(this, ctx);
            if (result.ErrorMessage != null)
                ctx.Offset = startIndex;

            return result;
        }
    }

    public class ParserContext
    {
        public string Input { get; }
        public int Offset { get; set; }
        public bool IsEOF => Offset == Input.Length;

        public ParserContext(string input)
        {
            Input = input;
            Offset = 0;
        }
    }

    public interface IPattern
    {
        PatternResult Match(Parser parser, ParserContext ctx);
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
