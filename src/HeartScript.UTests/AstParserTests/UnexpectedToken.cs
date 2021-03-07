using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.AstParserTests
{
    [TestFixture]
    public class UnexpectedToken
    {
        static readonly UnexpectedTokenTestCase[] s_testCases = new UnexpectedTokenTestCase[]
        {
            //ConstantConstant
            new UnexpectedTokenTestCase()
            {
                Infix = "1 1",
                ExpectedToken = new Token(Keyword.Constant, "1", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //IdentifierConstant
            new UnexpectedTokenTestCase()
            {
                Infix = "a 1",
                ExpectedToken = new Token(Keyword.Constant, "1", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //ConstantIdentifier
            new UnexpectedTokenTestCase()
            {
                Infix = "1 a",
                ExpectedToken = new Token(Keyword.Identifier, "a", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //IdentifierIdentifier
            new UnexpectedTokenTestCase()
            {
                Infix = "a a",
                ExpectedToken = new Token(Keyword.Identifier, "a", 2),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //RoundCloseIdentifier
            new UnexpectedTokenTestCase()
            {
                Infix = "(1) a",
                ExpectedToken = new Token(Keyword.Identifier, "a", 4),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //RoundCloseConstant
            new UnexpectedTokenTestCase()
            {
                Infix = "(a) 1",
                ExpectedToken = new Token(Keyword.Constant, "1", 4),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //MissingOpen
            new UnexpectedTokenTestCase()
            {
                Infix = "(1+2))",
                ExpectedToken = new Token(Keyword.RoundClose, ")", 5),
                ExpectedKeyword = Keyword.EndOfString,
            },
            //MissingClose
            new UnexpectedTokenTestCase()
            {
                Infix = "(1+2",
                ExpectedToken = new Token(Keyword.EndOfString, null, 4),
                ExpectedKeyword = Keyword.RoundClose,
            },
            //MissingCloseFunction
            new UnexpectedTokenTestCase()
            {
                Infix = "max(1,2",
                ExpectedToken = new Token(Keyword.EndOfString, null, 7),
                ExpectedKeyword = Keyword.RoundClose,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(UnexpectedTokenTestCase testCase)
        {
            var tokens = Lexer.Process(testCase.Infix);
            var ex = Assert.Throws<UnexpectedTokenException>(() => AstParser.Parse(DemoUtility.Operators, tokens));

            Assert.AreEqual(testCase.ExpectedToken, ex.Token);
            Assert.AreEqual(testCase.ExpectedKeyword, ex.ExpectedKeyword);
        }

        public struct UnexpectedTokenTestCase
        {
            public string Infix { get; set; }
            public Token ExpectedToken { get; set; }
            public Keyword? ExpectedKeyword { get; set; }
        }
    }
}
