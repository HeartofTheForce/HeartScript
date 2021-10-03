using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.End2EndTests
{
    [TestFixture]
    public class End2EndIntTests
    {
        static readonly End2EndTestCase[] s_testCases = new End2EndTestCase[]
        {
            //ReturnFloat2Int
            new End2EndTestCase()
            {
                Infix = "2.5 + 3.3",
                ExpectedNodeString = "(+ 2.5 3.3)",
            },
            //IntOnlyBitwise
            new End2EndTestCase()
            {
                Infix = "~(1 | 4)",
                ExpectedNodeString = "(~ (| 1 4))",
            },
            //BinaryPostfix
            new End2EndTestCase()
            {
                Infix = "2 * 1!",
                ExpectedNodeString = "(* 2 (! 1))",
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
