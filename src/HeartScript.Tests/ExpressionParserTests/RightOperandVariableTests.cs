using System.Collections.Generic;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionParserTests
{
    [TestFixture]
    public class RightOperandVariableTests
    {
        private static readonly IEnumerable<OperatorInfo> s_testOperators;

        static RightOperandVariableTests()
        {
            s_testOperators = OperatorInfoBuilder.Parse("./TestOperators/right-operand-variable.ops");
        }

        static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //{}
            new ExpressionTestCase()
            {
                Infix = "{x, y, z}",
                ExpectedOutput = "({ x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "{}",
                ExpectedOutput = "{",
            },
            new ExpressionTestCase()
            {
                Infix = "{x}",
                ExpectedOutput = "({ x)",
            },
            //[]
            new ExpressionTestCase()
            {
                Infix = "[x y z]",
                ExpectedOutput = "([ x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "[]",
                ExpectedOutput = "[",
            },
            new ExpressionTestCase()
            {
                Infix = "[x]",
                ExpectedOutput = "([ x)",
            },
            //?:
            new ExpressionTestCase()
            {
                Infix = "? x : y : z",
                ExpectedOutput = "(? x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "?",
                ExpectedOutput = "?",
            },
            new ExpressionTestCase()
            {
                Infix = "? x",
                ExpectedOutput = "(? x)",
            },
            //|
            new UnexpectedTokenTestCase()
            {
                Infix = "| x y z",
                ExpectedCharIndex = 4,
                ExpectedPattern = "EOF",
            },
            new ExpressionTestCase()
            {
                Infix = "|",
                ExpectedOutput = "|",
            },
            new ExpressionTestCase()
            {
                Infix = "| x",
                ExpectedOutput = "(| x)",
            },
            //&*
            new UnexpectedTokenTestCase()
            {
                Infix = "& x * y * z *",
                ExpectedCharIndex = 6,
                ExpectedPattern = "EOF",
            },
            new ExpressionTestCase()
            {
                Infix = "& *",
                ExpectedOutput = "&",
            },
            new ExpressionTestCase()
            {
                Infix = "& x *",
                ExpectedOutput = "(& x)",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_testOperators);
        }
    }
}
