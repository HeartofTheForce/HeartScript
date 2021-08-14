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
            //[]
            new ExpressionTestCase()
            {
                Infix = "[x y z]",
                ExpectedOutput = "([ x y z)",
            },
            //?:
            new ExpressionTestCase()
            {
                Infix = "? x : y : z",
                ExpectedOutput = "(? x y z)",
            },
            //|
            new ExpressionTestCase()
            {
                Infix = "| x y z",
                ExpectedOutput = "(| x y z)",
            },
            //&*
            new ExpressionTestCase()
            {
                Infix = "& x * y * z *",
                ExpectedOutput = "(& x y z)",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_testOperators);
        }
    }
}
