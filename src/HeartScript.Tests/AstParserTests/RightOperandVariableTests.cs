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

        static readonly AstTestCase[] s_testCases = new AstTestCase[]
        {
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
