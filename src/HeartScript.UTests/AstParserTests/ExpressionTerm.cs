using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.AstParserTests
{
    [TestFixture]
    public class ExpressionTerm
    {
        static readonly ExpressionTermTestCase[] s_testCases = new ExpressionTermTestCase[]
        {
            //EmptyDelimiterClose
            new ExpressionTermTestCase()
            {
                Infix = "max(1,2,)",
                ExpectedToken = new Token(Keyword.RoundClose, ")", 8),
            },
            //EmptyDelimiterDelimiter
            new ExpressionTermTestCase()
            {
                Infix = "max(1,,3)",
                ExpectedToken = new Token(Keyword.Comma, ",", 6),
            },
            //EmptyOpenDelimiter
            new ExpressionTermTestCase()
            {
                Infix = "max(,2,3)",
                ExpectedToken = new Token(Keyword.Comma, ",", 4),
            },
            //EmptyParentheses
            new ExpressionTermTestCase()
            {
                Infix = "()",
                ExpectedToken = new Token(Keyword.RoundClose, ")", 1),
            },
            //UnaryNoOperand
            new ExpressionTermTestCase()
            {
                Infix = "max(1, -)",
                ExpectedToken = new Token(Keyword.RoundClose, ")", 8),
            },
            //TooFewArgumentsBinary
            new ExpressionTermTestCase()
            {
                Infix = "1 +",
                ExpectedToken = new Token(Keyword.EndOfString, null, 3),
            },
            //TooFewArgumentsUnary
            new ExpressionTermTestCase()
            {
                Infix = "-",
                ExpectedToken = new Token(Keyword.EndOfString, null, 1),
            },
            //TooFewArgumentsBinaryChained
            new ExpressionTermTestCase()
            {
                Infix = "1 + 2 *",
                ExpectedToken = new Token(Keyword.EndOfString, null, 7),
            },
            //MissingCloseFunctionDelimiter
            new ExpressionTermTestCase()
            {
                Infix = "max(1,2,",
                ExpectedToken = new Token(Keyword.EndOfString, null, 8),
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ExpressionTermTestCase testCase)
        {
            var tokens = Lexer.Process(testCase.Infix);
            var ex = Assert.Throws<ExpressionTermException>(() => AstParser.Parse(DemoUtility.Operators, tokens));

            Assert.AreEqual(testCase.ExpectedToken, ex.Token);
        }

        public struct ExpressionTermTestCase
        {
            public string Infix { get; set; }
            public Token ExpectedToken { get; set; }
        }
    }
}
