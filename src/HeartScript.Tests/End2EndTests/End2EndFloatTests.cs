using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.End2EndTests
{
    [TestFixture]
    public class End2EndFloatTests
    {
        static readonly End2EndTestCase[] s_testCases = new End2EndTestCase[]
        {
            //LeftToFloat
            new End2EndTestCase()
            {
                Infix = "2 + 1.5",
                ExpectedNodeString = "(+ 2 1.5)",
            },
            //RightToFloat
            new End2EndTestCase()
            {
                Infix = "1.5 + 2",
                ExpectedNodeString = "(+ 1.5 2)",
            },
            //SinCosTan
            new End2EndTestCase()
            {
                Infix = "sin(1.0) + cos(1.0) + tan(1.0)",
                ExpectedNodeString = "(+ (+ ($ sin 1.0) ($ cos 1.0)) ($ tan 1.0))",
            },
            //MaxIntFloat
            new End2EndTestCase()
            {
                Infix = "max(2, b)",
                ExpectedNodeString = "($ max 2 b)",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase testCase)
        {
            testCase.Execute(Helper.TestOperators);
        }
    }
}
