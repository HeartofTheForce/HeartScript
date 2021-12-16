using NUnit.Framework;

namespace HeartScript.Tests.CompilerTests
{
    [TestFixture]
    public class PrefixTests
    {
        private static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
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
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
