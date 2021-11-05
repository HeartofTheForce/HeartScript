using HeartScript.Parsing;
using HeartScript.Peg;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class OperatorExpressionZeroTests
    {
        private static readonly PatternParser s_parser= PegHelper.BuildPatternParser("./TestOperators/operator-expression-zero.peg");

        static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //{}
            new ExpressionTestCase()
            {
                Infix = "{}",
                ExpectedOutput = "({)",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "{x}",
                ExpectedTextOffset = 1,
                ExpectedPattern = "}",
            },
            //[]
            new ExpressionTestCase()
            {
                Infix = "[]",
                ExpectedOutput = "([)",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "[x]",
                ExpectedTextOffset = 1,
                ExpectedPattern = "]",
            },
            //?:
            new ExpressionTestCase()
            {
                Infix = "?",
                ExpectedOutput = "(?)",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "? x",
                ExpectedTextOffset = 2,
                ExpectedPattern = "EOF",
            },
            //|
            new ExpressionTestCase()
            {
                Infix = "|",
                ExpectedOutput = "(|)",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "| x",
                ExpectedTextOffset = 2,
                ExpectedPattern = "EOF",
            },
            //&*
            new ExpressionTestCase()
            {
                Infix = "& *",
                ExpectedOutput = "(&)",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "& x *",
                ExpectedTextOffset = 2,
                ExpectedPattern = "*",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_parser);
        }
    }
}
