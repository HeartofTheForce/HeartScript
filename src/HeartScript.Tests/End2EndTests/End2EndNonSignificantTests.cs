using NUnit.Framework;

namespace HeartScript.Tests.End2EndTests
{
    [TestFixture]
    public class End2EndNonSignificantTests
    {
        static readonly End2EndTestCase[] s_testCases = new End2EndTestCase[]
        {
            //Leading Whitespace
            new End2EndTestCase()
            {
                Infix = " 1",
                ExpectedNodeString = "1",
            },
            //Trailing Whitespace
            new End2EndTestCase()
            {
                Infix = "1 ",
                ExpectedNodeString = "1",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase testCase)
        {
            testCase.Execute(Helper.Parser);
        }
    }
}
