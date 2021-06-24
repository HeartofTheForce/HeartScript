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

        private static bool TryMatch(string input, ref int offset, out Token token)
        {
            foreach (var pattern in s_patterns)
            {
                var match = pattern.Regex.Match(input, offset);
                if (match.Success)
                {
                    token = new Token(pattern.Keyword, match.Value, offset);
                    offset += match.Length;
                    return true;
                }
            }

            token = null!;
            return false;
        }

        public static IEnumerable<Token> Process(string input)
        {
            var output = new List<Token>();

            int offset = 0;
            while (offset < input.Length)
            {
                if (!TryMatch(input, ref offset, out var token))
                    throw new Exception($"No matching patterns '{offset}'");

                if (s_nonSignificantKeywords.Contains(token.Keyword))
                    continue;

                output.Add(token);
            }

            output.Add(new Token(Keyword.EndOfString, null!, offset));
            return output;
        }

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
