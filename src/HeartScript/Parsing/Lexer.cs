using System.Text.RegularExpressions;

namespace HeartScript.Parsing
{
    public class Lexer
    {
        public Token Current { get; private set; }
        public int Offset { get; private set; }

        private readonly string _input;

        public Lexer(string input)
        {
            Current = default!;
            Offset = 0;

            _input = input;

            var match = Regex.Match(input, "^\\s*");
            if (match.Success)
                Offset += match.Length;
        }

        public bool Eat(LexerPattern lexerPattern)
        {
            if (Offset == _input.Length)
            {
                if (Current == null || Current.Value != null)
                    Current = new Token(null, Offset);

                return false;
            }

            var match = lexerPattern.Regex.Match(_input, Offset);
            if (match.Success)
            {
                int groupIndex = match.Groups.Count - 1;
                Current = new Token(match.Groups[groupIndex].Value, match.Groups[groupIndex].Index);
                Offset += match.Length;
                return true;
            }

            return false;
        }
    }
}
