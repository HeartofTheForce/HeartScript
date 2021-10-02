using System.Collections.Generic;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class RightOperandZeroTests
    {
        private static readonly IEnumerable<OperatorPattern> s_testOperators;

        static RightOperandZeroTests()
        {
            s_testOperators = OperatorPatternBuilder.Parse("./TestOperators/right-operand-zero.ops");
        }

        static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //{}
            new ExpressionTestCase()
            {
                Infix = "{}",
                ExpectedOutput = "{",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "{x}",
                ExpectedCharIndex = 1,
                ExpectedPattern = "}",
            },
            //[]
            new ExpressionTestCase()
            {
                Infix = "[]",
                ExpectedOutput = "[",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "[x]",
                ExpectedCharIndex = 1,
                ExpectedPattern = "]",
            },
            //?:
            new ExpressionTestCase()
            {
                Infix = "?",
                ExpectedOutput = "?",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "? x",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //|
            new ExpressionTestCase()
            {
                Infix = "|",
                ExpectedOutput = "|",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "| x",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //&*
            new ExpressionTestCase()
            {
                Infix = "& *",
                ExpectedOutput = "&",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "& x *",
                ExpectedCharIndex = 2,
                ExpectedPattern = "*",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_testOperators);
        }
    }
}
