using System;
using System.Text.RegularExpressions;
using HeartScript.Nodes;

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

        public PatternResult Match(PatternParser parser, ParserContext ctx)
        {
            var match = Regex.Match(ctx.Input, ctx.Offset);
            if (match.Success)
            {
                if (match.Length == 0)
                    throw new Exception($"0 length match @ {match.Index}, {this}");

                var targetGroup = match.Groups[1];
                var token = new Token(targetGroup.Value, targetGroup.Index);
                ctx.Offset += match.Length;

                return PatternResult.Success(new PegNode(token.CharIndex, token.Value));
            }

            return PatternResult.Error(new UnexpectedTokenException(ctx.Offset, this));
        }
    }
}
