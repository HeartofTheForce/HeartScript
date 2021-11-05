using NUnit.Framework;

namespace HeartScript.Tests.CompilerTests
{
    [TestFixture]
    public class CompilerNullaryTests
    {
        static readonly IExpressionCompilerTestCase[] s_testCases = new IExpressionCompilerTestCase[]
        {
            //()
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "(1)",
                ExpectedString = "1",
                ExpectedExpression = () => 1,
            },
            //real
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "1.5",
                ExpectedString = "1.5",
                ExpectedExpression = () => 1.5,
            },
            //integral
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "1",
                ExpectedString = "1",
                ExpectedExpression = () => 1,
            },
            //boolean
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "true",
                ExpectedString = "true",
                ExpectedExpression = () => true,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "false",
                ExpectedString = "false",
                ExpectedExpression = () => false,
            },
            //identifier
            new ExpressionCompilerTestCase()
            {
                Method = "bool main(bool BoolA) => BoolA",
                Paramaters = new object[]
                {
                    true,
                },
                ExpectedResult = true,
            },
            new ExpressionCompilerTestCase()
            {
                Method = "int main(int IntA) => IntA",
                 Paramaters = new object[]
                {
                    1,
                },
                ExpectedResult = 1,
            },
            new ExpressionCompilerTestCase()
            {
                Method = "double main(double DoubleA) => DoubleA",
                Paramaters = new object[]
                {
                    1.5,
                },
                ExpectedResult = 1.5,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionCompilerTestCase testCase)
        {
            testCase.Execute(Helper.Parser);
        }
    }
}
