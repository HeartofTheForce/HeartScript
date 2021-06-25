using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.AstParserTests
{
    [TestFixture]
    public class UnexpectedTokenTests
    {
        static readonly UnexpectedTokenTestCase[] s_testCases = new UnexpectedTokenTestCase[]
        {
            //Constant Constant
            new UnexpectedTokenTestCase()
            {
                Infix = "1 1",
                UnexpectedToken = new Token(Keyword.Constant, "1", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //Identifier Constant
            new UnexpectedTokenTestCase()
            {
                Infix = "a 1",
                UnexpectedToken = new Token(Keyword.Constant, "1", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //Constant Identifier
            new UnexpectedTokenTestCase()
            {
                Infix = "1 a",
                UnexpectedToken = new Token(Keyword.Identifier, "a", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //Identifier Identifier
            new UnexpectedTokenTestCase()
            {
                Infix = "a a",
                UnexpectedToken = new Token(Keyword.Identifier, "a", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //RoundClose Identifier
            new UnexpectedTokenTestCase()
            {
                Infix = "(1) a",
                UnexpectedToken = new Token(Keyword.Identifier, "a", 4),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //RoundClose Constant
            new UnexpectedTokenTestCase()
            {
                Infix = "(a) 1",
                UnexpectedToken = new Token(Keyword.Constant, "1", 4),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //Unexpected Close
            new UnexpectedTokenTestCase()
            {
                Infix = "(1+2))",
                UnexpectedToken = new Token(Keyword.RoundClose, ")", 5),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //Bracket Missing Close
            new UnexpectedTokenTestCase()
            {
                Infix = "(1+2",
                UnexpectedToken = new Token(Keyword.EndOfString, null, 4),
                ExpectedKeyword = Keyword.RoundClose,
            },
            //Call Missing Close
            new UnexpectedTokenTestCase()
            {
                Infix = "max(1,2",
                UnexpectedToken = new Token(Keyword.EndOfString, null, 7),
                ExpectedKeyword = Keyword.RoundClose,
            },
            //Unexpected Comma
            new UnexpectedTokenTestCase()
            {
                Infix = "(1,)",
                UnexpectedToken = new Token(Keyword.Comma, ",", 2),
                ExpectedKeyword = Keyword.RoundClose,
            },
            //Too Few Operands
            new UnexpectedTokenTestCase()
            {
                Infix = "a ? b",
                UnexpectedToken = new Token(Keyword.EndOfString, null, 5),
                ExpectedKeyword = Keyword.Colon,
            },
            //Too Many Operands
            new UnexpectedTokenTestCase()
            {
                Infix = "a ? b : c : d",
                UnexpectedToken = new Token(Keyword.Colon, ":", 10),
                ExpectedKeyword = Keyword.EndOfString,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(UnexpectedTokenTestCase testCase)
        {
            var lexer = new Lexer(testCase.Infix);
            var ex = Assert.Throws<UnexpectedTokenException>(() => AstParser.Parse(Demo.Operators, lexer));

            Assert.AreEqual(testCase.UnexpectedToken, ex.Token);
            Assert.AreEqual(testCase.ExpectedKeyword, ex.ExpectedKeyword);
        }

        public struct UnexpectedTokenTestCase
        {
            public string Infix { get; set; }
            public Token UnexpectedToken { get; set; }
            public Keyword? ExpectedKeyword { get; set; }

            public override string ToString()
            {
                return $"\"{Infix}\"";
            }
        }
    }
}
