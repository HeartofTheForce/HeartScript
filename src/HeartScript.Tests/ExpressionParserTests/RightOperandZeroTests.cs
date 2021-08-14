using System.Collections.Generic;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionParserTests
{
    [TestFixture]
    public class RightOperandZeroTests
    {
        private static readonly IEnumerable<OperatorInfo> s_testOperators;

        static RightOperandZeroTests()
        {
            s_testOperators = OperatorInfoBuilder.Parse("./TestOperators/right-operand-zero.ops");
        }

        static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_testOperators);
        }
    }
}
