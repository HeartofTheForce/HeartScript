using System.Collections.Generic;
using HeartScript.Expressions;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class OperatorExpressionFixedTests
    {
        private static readonly IEnumerable<OperatorInfo> s_testOperators;

        static OperatorExpressionFixedTests()
        {
            s_testOperators = OperatorInfoBuilder.Parse("./TestOperators/operator-expression-fixed.ops");
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
                ExpectedCharIndex = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "{x, y, z, w}",
                ExpectedCharIndex = 8,
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
                ExpectedCharIndex = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "[x y z w]",
                ExpectedCharIndex = 7,
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
                ExpectedCharIndex = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "? x : y : z : w",
                ExpectedCharIndex = 12,
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
                ExpectedCharIndex = 1,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "| x y z w",
                ExpectedCharIndex = 8,
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
                ExpectedCharIndex = 2,
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "& x * y * z * w *",
                ExpectedCharIndex = 14,
                ExpectedPattern = "EOF",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_testOperators);
        }
    }
}