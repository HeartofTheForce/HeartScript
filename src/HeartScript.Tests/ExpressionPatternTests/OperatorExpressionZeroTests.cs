using System.Collections.Generic;
using HeartScript.Expressions;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class OperatorExpressionZeroTests
    {
        private static readonly IEnumerable<OperatorInfo> s_testOperators;

        static OperatorExpressionZeroTests()
        {
            s_testOperators = OperatorInfoBuilder.Parse("./TestOperators/operator-expression-zero.ops");
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
