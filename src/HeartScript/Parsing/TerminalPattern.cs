using System.Text.RegularExpressions;

namespace HeartScript.Parsing
{
    public class TerminalPattern : IPattern
    {
        public Regex Regex { get; }
        public string Pattern { get; }
        public bool IsRegex { get; }

        private TerminalPattern(string pattern, bool isRegex)
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

        public static TerminalPattern FromRegex(string pattern)
        {
            return new TerminalPattern(pattern, true);
        }

        public static TerminalPattern FromPlainText(string pattern)
        {
            return new TerminalPattern(pattern, false);
        }

        public override string ToString()
        {
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

    public class ValueNode : IParseNode
    {
        public int TextOffset { get; }
        public string Value { get; }

        public ValueNode(int textOffset, string value)
        {
            TextOffset = textOffset;
            Value = value;
        }
    }

    public class UnexpectedTokenException : PatternException
    {
        public string ExpectedPattern { get; }

        public UnexpectedTokenException(int textOffset, string expectedPattern) : base(textOffset, $"Unexpected Token @ {textOffset} expected {expectedPattern}")
        {
            ExpectedPattern = expectedPattern;
        }

        public UnexpectedTokenException(int textOffset, TerminalPattern terminalPattern) : this(textOffset, terminalPattern.ToString())
        {
        }
    }
}
