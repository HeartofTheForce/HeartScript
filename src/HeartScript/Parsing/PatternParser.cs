using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class PatternParser
    {
        public Dictionary<string, IPattern> Patterns { get; }

        public PatternParser()
        {
            Patterns = new Dictionary<string, IPattern>();
        }

        public PatternResult TryMatch(IPattern pattern, ParserContext ctx)
        {
            int startIndex = ctx.Offset;

            var result = pattern.Match(this, ctx);
            if (result.Node == null)
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
        PatternResult Match(PatternParser parser, ParserContext ctx);
    }

    public class PatternResult
    {
        public INode? Node { get; private set; }
        public PatternException? Exception { get; private set; }

        public static PatternResult Success(INode node)
        {
            return new PatternResult()
            {
                Node = node,
            };
        }

        public static PatternResult Error(PatternException exception)
        {
            return new PatternResult()
            {
                Exception = exception,
            };
        }
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
