using System;

namespace HeartScript.Parsing
{
    public class Lexer
    {
        private static readonly LexerPattern s_nonSignificant = new LexerPattern("\\s*", true);

        public Token Current { get; private set; }
        public int Offset { get; private set; }
        public bool IsEOF => Offset == _input.Length;

        private readonly string _input;

        public Lexer(string input)
        {
            Current = default!;
            Offset = 0;

            _input = input;
        }

        public bool Eat(LexerPattern lexerPattern)
        {
            var nonSignificantMatch = s_nonSignificant.Regex.Match(_input, Offset);
            if (nonSignificantMatch.Success)
                Offset += nonSignificantMatch.Length;

            var match = lexerPattern.Regex.Match(_input, Offset);
            if (match.Success)
            {
                if (match.Groups[1].Length == 0)
                    throw new Exception($"0 length match @ {match.Groups[1].Index}, {lexerPattern}");

                int groupIndex = match.Groups.Count - 1;
                Current = new Token(match.Groups[groupIndex].Value, match.Groups[groupIndex].Index);
                Offset += match.Length;
                return true;
            }

            return false;
        }
    }
}
