using System.Collections.Generic;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.AstParserTests
{
    [TestFixture]
    public class RightOperandFixedTests
    {
        private static readonly IEnumerable<OperatorInfo> s_testOperators;

        static RightOperandFixedTests()
        {
            s_testOperators = OperatorInfoBuilder.Parse("./TestOperators/right-operand-fixed.ops");
        }

        static readonly AstTestCase[] s_testCases = new AstTestCase[]
        {
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(AstTestCase testCase)
        {
            var lexer = new Lexer(testCase.Infix);

            var node = AstParser.Parse(s_testOperators, lexer);
            Assert.AreEqual(testCase.ExpectedOutput, node.ToString());
        }
    }
}
