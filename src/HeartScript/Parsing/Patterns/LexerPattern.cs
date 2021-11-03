using System.Text.RegularExpressions;
using HeartScript.Parsing.Nodes;

namespace HeartScript.Parsing
{
    public class LexerPattern : IPattern
    {
        public Regex Regex { get; }
        public string Pattern { get; }
        public bool IsRegex { get; }

        private LexerPattern(string pattern, bool isRegex)
        {
            Pattern = pattern;
            IsRegex = isRegex;

            Regex temp;
            if (IsRegex)
                temp = new Regex(pattern);
            else
                temp = new Regex(Regex.Escape(pattern));

            Regex = new Regex($"\\G({temp})");
        }

        public static LexerPattern FromRegex(string pattern)
        {
            return new LexerPattern(pattern, true);
        }

        public static LexerPattern FromPlainText(string pattern)
        {
            return new LexerPattern(pattern, false);
        }

        public override string ToString()
        {
            if (IsRegex)
                return $"Regex: {Pattern}";
            else
                return Pattern;
        }

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            var match = Regex.Match(ctx.Input, ctx.Offset);
            if (match.Success)
            {
                var targetGroup = match.Groups[1];
                ctx.Offset += match.Length;

                return new ValueNode(targetGroup.Index, targetGroup.Value);
            }

            ctx.LogException(new UnexpectedTokenException(ctx.Offset, this));
            return null;
        }
    }

    public class UnexpectedTokenException : PatternException
    {
        public string ExpectedPattern { get; }

        public UnexpectedTokenException(int textOffset, string expectedPattern) : base(textOffset, $"Unexpected Token @ {textOffset} expected {expectedPattern}")
        {
            ExpectedPattern = expectedPattern;
        }

        public UnexpectedTokenException(int textOffset, LexerPattern lexerPattern) : this(textOffset, lexerPattern.ToString())
        {
        }
    }
}
