using System;
using System.Collections.Generic;

namespace HeartScript.Parsing
{
    public class PatternParser
    {
        public Dictionary<string, IPattern> Patterns { get; }

        public PatternParser()
        {
            Patterns = new Dictionary<string, IPattern>();
        }

        public INode? TryMatch(IPattern pattern, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var result = pattern.Match(this, ctx);
            if (result == null)
                ctx.Offset = localOffset;

            return result;
        }
    }

    public class ParserContext
    {
        public string Input { get; }
        public int Offset { get; set; }
        public PatternException? Exception { get; set; }
        public bool IsEOF => Offset == Input.Length;

        public ParserContext(string input)
        {
            Input = input;
            Offset = 0;
            Exception = null;
        }
    }

    public interface IPattern
    {
        INode? Match(PatternParser parser, ParserContext ctx);
    }

    public abstract class PatternException : Exception
    {
        public int CharIndex { get; }

        public PatternException(int charIndex, string message) : base(message)
        {
            CharIndex = charIndex;
        }
    }
}
