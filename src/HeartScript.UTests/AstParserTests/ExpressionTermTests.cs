using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.AstParserTests
{
    [TestFixture]
    public class ExpressionTerm
    {
        static readonly ExpressionTermTestCase[] s_testCases = new ExpressionTermTestCase[]
        {
            //Empty Delimiter Close
            new ExpressionTermTestCase()
            {
                Infix = "max(1,2,)",
                ExpectedToken = new Token(Keyword.RoundClose, ")", 8),
            },
            //Empty Delimiter Delimiter
            new ExpressionTermTestCase()
            {
                Infix = "max(1,,3)",
                ExpectedToken = new Token(Keyword.Comma, ",", 6),
            },
            //Empty Open Delimiter
            new ExpressionTermTestCase()
            {
                Infix = "max(,2,3)",
                ExpectedToken = new Token(Keyword.Comma, ",", 4),
            },
            //Empty Delimiter EndOfString
            new ExpressionTermTestCase()
            {
                Infix = "max(1,2,",
                ExpectedToken = new Token(Keyword.EndOfString, null, 8),
            },
            //Empty Brackets
            new ExpressionTermTestCase()
            {
                Infix = "()",
                ExpectedToken = new Token(Keyword.RoundClose, ")", 1),
            },
            //Empty Binary Right
            new ExpressionTermTestCase()
            {
                Infix = "1 +",
                ExpectedToken = new Token(Keyword.EndOfString, null, 3),
            },
            //Empty Unary Right
            new ExpressionTermTestCase()
            {
                Infix = "-",
                ExpectedToken = new Token(Keyword.EndOfString, null, 1),
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

            public override string ToString()
            {
                return $"\"{Infix}\"";
            }
        }
    }
}
