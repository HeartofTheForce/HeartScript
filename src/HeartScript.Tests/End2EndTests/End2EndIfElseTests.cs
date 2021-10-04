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
            testCase.Execute(Helper.TestOperators);
        }
    }
}
