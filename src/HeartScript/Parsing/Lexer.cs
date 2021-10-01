using System;

namespace HeartScript.Parsing
{
    public class Lexer
    {
        private static readonly LexerPattern s_nonSignificant = LexerPattern.FromRegex("\\s*");

        public int Offset { get; set; }
        public bool IsEOF => Offset == _input.Length;

        private readonly string _input;

        public Lexer(string input)
        {
            Offset = 0;

            _input = input;

            var nonSignificantMatch = s_nonSignificant.Regex.Match(_input, Offset);
            if (nonSignificantMatch.Success)
                Offset += nonSignificantMatch.Length;
        }

        public bool TryEat(LexerPattern lexerPattern, out Token? value)
        {
            value = null;

            var match = lexerPattern.Regex.Match(_input, Offset);
            if (match.Success)
            {
                if (match.Length == 0)
                    throw new Exception($"0 length match @ {match.Index}, {lexerPattern}");

                var targetGroup = match.Groups[1];
                value = new Token(targetGroup.Value, targetGroup.Index);
                Offset += match.Length;
            }

            var nonSignificantMatch = s_nonSignificant.Regex.Match(_input, Offset);
            if (nonSignificantMatch.Success)
                Offset += nonSignificantMatch.Length;

            return match.Success;
        }
    }
}
