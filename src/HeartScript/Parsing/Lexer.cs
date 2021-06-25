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
                if (Current.Keyword == Keyword.EndOfString)
                    return false;

                Current = new Token(Keyword.EndOfString, null!, _offset);
            }
            else
            {
                do
                {
                    if (!TryMatch())
                        throw new Exception($"No matching patterns @ {_offset}");
                } while (s_nonSignificantKeywords.Contains(Current.Keyword));
            }

            return true;
        }

        private bool TryMatch()
        {
            foreach (var pattern in s_patterns)
            {
                var match = pattern.Regex.Match(_input, _offset);
                if (match.Success)
                {
                    Current = new Token(pattern.Keyword, match.Value, _offset);
                    _offset += match.Length;
                    return true;
                }
            }

            return false;
        }

        private static readonly Pattern[] s_patterns = new Pattern[]
        {
            new Pattern("(\r\n|\r|\n)", Keyword.Newline),
            new Pattern(" +", Keyword.Space),
            new Pattern("\t+", Keyword.Tab),
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

        private static readonly Keyword[] s_nonSignificantKeywords = new Keyword[]
        {
            Keyword.Space,
            Keyword.Tab,
            Keyword.Newline,
        };

        private struct Pattern
        {
            public Regex Regex { get; }
            public Keyword Keyword { get; }

            public Pattern(string regex, Keyword keyword)
            {
                Regex = new Regex($"\\G{regex}");
                Keyword = keyword;
            }
        }
    }
}
