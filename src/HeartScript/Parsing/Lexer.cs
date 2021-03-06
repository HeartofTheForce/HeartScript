using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeartScript.Parsing
{
    public static class Lexer
    {
        private static readonly Pattern[] s_patterns = new Pattern[]
        {
            new Pattern()
            {
                Regex = new Regex("(\r\n|\r|\n)"),
                Keyword = Keyword.Newline,
            },
            new Pattern()
            {
                Regex = new Regex(" +"),
                Keyword = Keyword.Space,
            },
            new Pattern()
            {
                Regex = new Regex("\t+"),
                Keyword = Keyword.Tab,
            },
            new Pattern()
            {
                Regex = new Regex("\\("),
                Keyword = Keyword.RoundOpen,
            },
            new Pattern()
            {
                Regex = new Regex("\\)"),
                Keyword = Keyword.RoundClose,
            },
            new Pattern()
            {
                Regex = new Regex(","),
                Keyword = Keyword.Comma,
            },
            new Pattern()
            {
                Regex = new Regex("\\+"),
                Keyword = Keyword.Plus,
            },
            new Pattern()
            {
                Regex = new Regex("-"),
                Keyword = Keyword.Minus,
            },
            new Pattern()
            {
                Regex = new Regex("\\*"),
                Keyword = Keyword.Asterisk,
            },
            new Pattern()
            {
                Regex = new Regex("/"),
                Keyword = Keyword.ForwardSlash,
            },
            new Pattern()
            {
                Regex = new Regex("\\d+(?:\\.\\d+)?"),
                Keyword = Keyword.Constant,
            },
            new Pattern()
            {
                Regex = new Regex("[a-zA-Z]\\w*"),
                Keyword = Keyword.Identifier,
            },
        };

        private static readonly Keyword[] s_nonSignificantKeywords = new Keyword[]
        {
            Keyword.Space,
            Keyword.Tab,
            Keyword.Newline,
        };

        private static bool TryMatch(
            string input,
            int line,
            int column,
            ref int offset,
            out Token token)
        {
            foreach (var pattern in s_patterns)
            {
                var match = pattern.Regex.Match(input, offset);
                if (match.Success && match.Index == offset)
                {
                    offset += match.Length;
                    token = new Token(pattern.Keyword, match.Value, line, column);
                    return true;
                }
            }

            token = null!;
            return false;
        }

        public static IEnumerable<Token> Process(string input)
        {
            var output = new List<Token>();

            int line = 0;
            int column = 0;
            int offset = 0;
            while (offset < input.Length)
            {
                if (!TryMatch(input, line, column, ref offset, out var token))
                    throw new Exception($"No matching patterns '{offset}'");

                if (token.Keyword == Keyword.Newline)
                {
                    line++;
                    column = 0;
                }
                else
                {
                    column++;
                }

                if (s_nonSignificantKeywords.Contains(token.Keyword))
                    continue;

                output.Add(token);
            }

            return output;
        }

        private struct Pattern
        {
            public Regex Regex { get; set; }
            public Keyword Keyword { get; set; }
        }
    }
}
