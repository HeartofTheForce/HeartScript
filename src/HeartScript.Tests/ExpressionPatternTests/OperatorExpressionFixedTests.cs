using HeartScript.Parsing;
using HeartScript.Peg;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class OperatorExpressionFixedTests
    {
        private static readonly PatternParser s_parser;

        static OperatorExpressionFixedTests()
        {
            s_parser = PegHelper.BuildPatternParser("./TestOperators/operator-expression-fixed.peg");
        }

        static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //{}
            new ExpressionTestCase()
            {
                Infix = "{x, y, z}",
                ExpectedOutput = "({ x y z)",
            },
            new ExpressionTermTestCase()
            {
                Infix = "{}",
                ExpectedTextOffset = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "{x, y, z, w}",
                ExpectedTextOffset = 8,
                ExpectedPattern = "}",
            },
            //[]
            new ExpressionTestCase()
            {
                Infix = "[x y z]",
                ExpectedOutput = "([ x y z)",
            },
            new ExpressionTermTestCase()
            {
                Infix = "[]",
                ExpectedTextOffset = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "[x y z w]",
                ExpectedTextOffset = 7,
                ExpectedPattern = "]",
            },
            //?:
            new ExpressionTestCase()
            {
                Infix = "? x : y : z",
                ExpectedOutput = "(? x y z)",
            },
            new ExpressionTermTestCase()
            {
                Infix = "?",
                ExpectedTextOffset = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "? x : y : z : w",
                ExpectedTextOffset = 12,
                ExpectedPattern = "EOF",
            },
            //|
            new ExpressionTestCase()
            {
                Infix = "| x y z",
                ExpectedOutput = "(| x y z)",
            },
            new ExpressionTermTestCase()
            {
                Infix = "|",
                ExpectedTextOffset = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "| x y z w",
                ExpectedTextOffset = 8,
                ExpectedPattern = "EOF",
            },
            //&*
            new ExpressionTestCase()
            {
                Infix = "& x * y * z *",
                ExpectedOutput = "(& x y z)",
            },
            new ExpressionTermTestCase()
            {
                Infix = "& *",
                ExpectedTextOffset = 2,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "& x * y * z * w *",
                ExpectedTextOffset = 14,
                ExpectedPattern = "EOF",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_parser);
        }
    }
}
