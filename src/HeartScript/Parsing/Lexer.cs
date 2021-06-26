using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeartScript.Parsing
{
    public class Lexer
    {
        public Token Current { get; private set; } = default!;

        private readonly string _input;
        private int _offset;

        public Lexer(string input)
        {
            _input = input;
            _offset = 0;
        }

        public bool Eat(LexerPattern lexerPattern)
        {
            if (_offset == _input.Length)
            {
                if (Current?.CharOffset != _offset)
                    return false;

                Current = new Token("EOF", _offset);
                return true;
            }

            var match = lexerPattern.Regex.Match(_input, _offset);
            if (match.Success)
            {
                Current = new Token(match.Groups[1].Value, match.Groups[1].Index);
                _offset += match.Length;
                return true;
            }

            return false;
        }

        private static readonly LexerPattern[] s_patterns = new LexerPattern[]
        {
            new LexerPattern("(", false),
            new LexerPattern(")", false),
            new LexerPattern(",", false),
            new LexerPattern("!", false),
            new LexerPattern("+", false),
            new LexerPattern("-", false),
            new LexerPattern("*", false),
            new LexerPattern("/", false),
            new LexerPattern("~", false),
            new LexerPattern("&", false),
            new LexerPattern("^", false),
            new LexerPattern("|", false),
            new LexerPattern("?", false),
            new LexerPattern(":", false),
            new LexerPattern("if", false),
            new LexerPattern("else", false),
            new LexerPattern("\\d+(?:\\.\\d+)?", true),
            new LexerPattern("[a-zA-Z]\\w*", true),
        };

        public class LexerPattern
        {
            public Regex Regex { get; }
            public string Pattern { get; }
            public bool IsRegex { get; }

            public LexerPattern(string pattern, bool isRegex)
            {
                Pattern = pattern;
                IsRegex = isRegex;

                if (IsRegex)
                    Regex = new Regex($"\\G\\s*({pattern})");
                else
                    Regex = new Regex($"\\G\\s*({Regex.Escape(pattern)})");
            }
        }
    }
}
