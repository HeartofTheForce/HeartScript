using System.Collections.Generic;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.AstParserTests
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
            new AstTestCase()
            {
                Infix = "{x, y, z}",
                ExpectedOutput = "({ x y z)",
            },
            new AstTestCase()
            {
                Infix = "{}",
                ExpectedOutput = "{",
            },
            new AstTestCase()
            {
                Infix = "{x}",
                ExpectedOutput = "({ x)",
            },
            //[]
            new AstTestCase()
            {
                Infix = "[x y z]",
                ExpectedOutput = "([ x y z)",
            },
            new AstTestCase()
            {
                Infix = "[]",
                ExpectedOutput = "[",
            },
            new AstTestCase()
            {
                Infix = "[x]",
                ExpectedOutput = "([ x)",
            },
            //?:
            new AstTestCase()
            {
                Infix = "? x : y : z",
                ExpectedOutput = "(? x y z)",
            },
            new AstTestCase()
            {
                Infix = "?",
                ExpectedOutput = "?",
            },
            new AstTestCase()
            {
                Infix = "? x",
                ExpectedOutput = "(? x)",
            },
            //|
            new AstTestCase()
            {
                Infix = "| x",
                ExpectedOutput = "(| x)",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "| x y z",
                ExpectedCharIndex = 3,
                ExpectedPattern = "EOF",
            },
            //&*
            new AstTestCase()
            {
                Infix = "& x *",
                ExpectedOutput = "(& x)",
            },
            new UnexpectedTokenTestCase()
            {
                Infix = "& x * y * z &",
                ExpectedCharIndex = 5,
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
