using System.Collections.Generic;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionParserTests
{
    [TestFixture]
    public class RightOperandFixedTests
    {
        private static readonly IEnumerable<OperatorInfo> s_testOperators;

        static RightOperandFixedTests()
        {
            s_testOperators = OperatorInfoBuilder.Parse("./TestOperators/right-operand-fixed.ops");
        }

        static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //{}
            new ExpressionTestCase()
            {
                Infix = "{x, y, z}",
                ExpectedOutput = "({ x y z)",
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
            //|
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
