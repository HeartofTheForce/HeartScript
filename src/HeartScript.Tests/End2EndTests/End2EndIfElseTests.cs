using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.End2EndTests
{
    [TestFixture]
    public class End2EndIfElseTests
    {
        static readonly End2EndTestCase[] s_testCases = new End2EndTestCase[]
        {
            //IfElseTernary
            new End2EndTestCase()
            {
                Infix = "if a ? b : c d ? e : f else g ? h : i",
                ExpectedNodeString = "(else (if (? a b c) (? d e f)) (? g h i))",
            },
            //ChainedIfElse
            new End2EndTestCase()
            {
                Infix = "if a b else if c d else e",
                ExpectedNodeString = "(else (if a b) (else (if c d) e))",
            },
            //If
            new End2EndTestCase()
            {
                Infix = "if a b",
                ExpectedNodeString = "(if a b)",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase testCase)
        {
            var ctx = new ParserContext(testCase.Infix);

            var node = ExpressionPattern.Parse(Helper.TestOperators, ctx);
            Assert.AreEqual(testCase.ExpectedNodeString, node.ToString());
        }
    }
}
