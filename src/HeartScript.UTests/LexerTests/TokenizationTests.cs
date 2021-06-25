using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.LexerTests
{
    [TestFixture]
    public class TokenizationTests
    {
        static readonly TokenizationTestCase[] s_testCases = new TokenizationTestCase[]
        {
            //NoWhitespace
            new TokenizationTestCase()
            {
                Infix = "(-1+a)",
                ExpectedTokens = new Token[]
                {
                    new Token(Keyword.RoundOpen, "(", 0),
                    new Token(Keyword.Minus, "-", 1),
                    new Token(Keyword.Constant, "1", 2),
                    new Token(Keyword.Plus, "+", 3),
                    new Token(Keyword.Identifier, "a", 4),
                    new Token(Keyword.RoundClose, ")", 5),
                    new Token(Keyword.EndOfString, null, 6),
                },
            },
            //WhitespaceExcluded
            new TokenizationTestCase()
            {
                Infix = "( - 1 + a )",
                ExpectedTokens = new Token[]
                {
                    new Token(Keyword.RoundOpen, "(", 0),
                    new Token(Keyword.Minus, "-", 2),
                    new Token(Keyword.Constant, "1", 4),
                    new Token(Keyword.Plus, "+", 6),
                    new Token(Keyword.Identifier, "a", 8),
                    new Token(Keyword.RoundClose, ")", 10),
                    new Token(Keyword.EndOfString, null, 11),
                },
            },
            //IdentifierNumberSuffix
            new TokenizationTestCase()
            {
                Infix = "a0 * a1 & 0",
                ExpectedTokens = new Token[]
                {
                    new Token(Keyword.Identifier, "a0", 0),
                    new Token(Keyword.Multiply, "*", 3),
                    new Token(Keyword.Identifier, "a1", 5),
                    new Token(Keyword.BitwiseAnd, "&", 8),
                    new Token(Keyword.Constant, "0", 10),
                    new Token(Keyword.EndOfString, null, 11),
                },
            },
            //DelimiterWhitespace
            new TokenizationTestCase()
            {
                Infix = "min(1 + a , 5.3)",
                ExpectedTokens = new Token[]
                {
                    new Token(Keyword.Identifier, "min", 0),
                    new Token(Keyword.RoundOpen, "(", 3),
                    new Token(Keyword.Constant, "1", 4),
                    new Token(Keyword.Plus, "+", 6),
                    new Token(Keyword.Identifier, "a", 8),
                    new Token(Keyword.Comma, ",", 10),
                    new Token(Keyword.Constant, "5.3", 12),
                    new Token(Keyword.RoundClose, ")", 15),
                    new Token(Keyword.EndOfString, null, 16),
                },
            },
            //DelimiterNoWhitespace
            new TokenizationTestCase()
            {
                Infix = "max(a,sin(c),d)",
                ExpectedTokens = new Token[]
                {
                    new Token(Keyword.Identifier, "max", 0),
                    new Token(Keyword.RoundOpen, "(", 3),
                    new Token(Keyword.Identifier, "a", 4),
                    new Token(Keyword.Comma, ",", 5),
                    new Token(Keyword.Identifier, "sin", 6),
                    new Token(Keyword.RoundOpen, "(", 9),
                    new Token(Keyword.Identifier, "c", 10),
                    new Token(Keyword.RoundClose, ")", 11),
                    new Token(Keyword.Comma, ",", 12),
                    new Token(Keyword.Identifier, "d", 13),
                    new Token(Keyword.RoundClose, ")", 14),
                    new Token(Keyword.EndOfString, null, 15),
                },
            },
            //Empty
            new TokenizationTestCase()
            {
                Infix = "",
                ExpectedTokens = new Token[]
                {
                    new Token(Keyword.EndOfString, null, 0),
                },
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(TokenizationTestCase testCase)
        {
            var lexer = new Lexer(testCase.Infix);

            int tokenCount = 0;
            while (lexer.MoveNext())
            {
                Assert.AreEqual(testCase.ExpectedTokens[tokenCount], lexer.Current);
                tokenCount++;
            }

            Assert.AreEqual(testCase.ExpectedTokens.Length, tokenCount);
        }

        public struct TokenizationTestCase
        {
            public string Infix { get; set; }
            public Token[] ExpectedTokens { get; set; }
        }
    }
}
