using NUnit.Framework;

namespace HeartScript.Tests.ExpressionTests
{
    [TestFixture]
    public class PrefixTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //u+
            new ExpressionTestCase<int>()
            {
                Infix = "+1",
                ExpectedExpression = () => +1,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "+1.5",
                ExpectedExpression = () => +1.5,
            },
            //u-
            new ExpressionTestCase<int>()
            {
                Infix = "-1",
                ExpectedExpression = () => -1,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "-1.5",
                ExpectedExpression = () => -1.5,
            },
            //~
            new ExpressionTestCase<int>()
            {
                Infix = "~1",
                ExpectedExpression = () => ~1,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
