using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.End2EndTests
{
    [TestFixture]
    public class End2EndTernaryTests
    {
        static readonly End2EndTestCase[] s_testCases = new End2EndTestCase[]
        {
            //ChainedTernary
            new End2EndTestCase()
            {
                Infix = "a ? b : c ? d : e",
                ExpectedNodeString = "(? a b (? c d e))",
            },
            //TernaryBrackets
            new End2EndTestCase()
            {
                Infix = "(a ? b : c) ? d : e",
                ExpectedNodeString = "(? (? a b c) d e)",
            },
            //TernaryBinary
            new End2EndTestCase()
            {
                Infix = "a ? b : c + 1",
                ExpectedNodeString = "(? a b (+ c 1))",
            },
            //NestedTernary
            new End2EndTestCase()
            {
                Infix = "a ? b ? c : d : e",
                ExpectedNodeString = "(? a (? b c d) e)",
            },
            //NestedTernary
            new End2EndTestCase()
            {
                Infix = "a * b ? c : d",
                ExpectedNodeString = "(* a (? b c d))",
            },
            //NestedTernary
            new End2EndTestCase()
            {
                Infix = "-a ? b : c",
                ExpectedNodeString = "(? (- a) b c)",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase testCase)
        {
            var lexer = new Lexer(testCase.Infix);

            var node = AstParser.Parse(Helper.TestOperators, lexer);
            Assert.AreEqual(testCase.ExpectedNodeString, node.ToString());
        }
    }
}
