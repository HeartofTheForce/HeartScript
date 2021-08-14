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

        static readonly ExpressionTestCase[] s_testCases = new ExpressionTestCase[]
        {
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ExpressionTestCase testCase)
        {
            var lexer = new Lexer(testCase.Infix);

            var node = ExpressionParser.Parse(s_testOperators, lexer);
            Assert.AreEqual(testCase.ExpectedOutput, node.ToString());
        }
    }
}
