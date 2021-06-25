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

        public bool MoveNext()
        {
            if (_offset == _input.Length)
            {
                if (Current?.Keyword == Keyword.EndOfString)
                    return false;

                Current = new Token(Keyword.EndOfString, null!, _offset);
                return true;
            }

            return TryMatch();
        }

        private bool TryMatch()
        {
            foreach (var pattern in s_patterns)
            {
                var match = pattern.Regex.Match(_input, _offset);
                if (match.Success)
                {
                    Current = new Token(pattern.Keyword, match.Groups[1].Value, match.Groups[1].Index);
                    _offset += match.Length;
                    return true;
                }
            }

            return false;
        }

        private static readonly Pattern[] s_patterns = new Pattern[]
        {
            new Pattern("\\(", Keyword.RoundOpen),
            new Pattern("\\)", Keyword.RoundClose),
            new Pattern(",", Keyword.Comma),
            new Pattern("!", Keyword.Factorial),
            new Pattern("\\+", Keyword.Plus),
            new Pattern("-", Keyword.Minus),
            new Pattern("\\*", Keyword.Multiply),
            new Pattern("/", Keyword.Divide),
            new Pattern("~", Keyword.BitwiseNot),
            new Pattern("&", Keyword.BitwiseAnd),
            new Pattern("\\^", Keyword.BitwiseXor),
            new Pattern("\\|", Keyword.BitwiseOr),
            new Pattern("\\?", Keyword.Ternary),
            new Pattern(":", Keyword.Colon),
            new Pattern("\\d+(?:\\.\\d+)?", Keyword.Constant),
            new Pattern("if", Keyword.If),
            new Pattern("else", Keyword.Else),
            new Pattern("[a-zA-Z]\\w*", Keyword.Identifier),
        };

        private struct Pattern
        {
            public Regex Regex { get; }
            public Keyword Keyword { get; }

            public Pattern(string pattern, Keyword keyword)
            {
                Regex = new Regex($"\\G\\s*({pattern})");
                Keyword = keyword;
            }
        }
    }
}
