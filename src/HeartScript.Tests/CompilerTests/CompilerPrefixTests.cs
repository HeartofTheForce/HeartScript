using NUnit.Framework;

namespace HeartScript.Tests.CompilerTests
{
    [TestFixture]
    public class CompilerPrefixTests
    {
        static readonly IExpressionCompilerTestCase[] s_testCases = new IExpressionCompilerTestCase[]
        {
            //u+
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "+1",
                ExpectedString = "(u+ 1)",
                ExpectedExpression = () => +1,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "+1.5",
                ExpectedString = "(u+ 1.5)",
                ExpectedExpression = () => +1.5,
            },
            //u-
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "-1",
                ExpectedString = "(u- 1)",
                ExpectedExpression = () => -1,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "-1.5",
                ExpectedString = "(u- 1.5)",
                ExpectedExpression = () => -1.5,
            },
            //~
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "~1",
                ExpectedString = "(~ 1)",
                ExpectedExpression = () => ~1,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionCompilerTestCase testCase)
        {
            testCase.Execute(Helper.TestOperators);
        }
    }
}
