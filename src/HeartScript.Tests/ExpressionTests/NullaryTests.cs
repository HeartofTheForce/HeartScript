using NUnit.Framework;

namespace HeartScript.Tests.ExpressionTests
{
    [TestFixture]
    public class NullaryTests
    {
        private static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //()
            new ExpressionTestCase<int>()
            {
                Infix = "(1)",
                ExpectedExpression = () => 1,
            },
            //real
            new ExpressionTestCase<double>()
            {
                Infix = "1.5",
                ExpectedExpression = () => 1.5,
            },
            //integral
            new ExpressionTestCase<int>()
            {
                Infix = "1",
                ExpectedExpression = () => 1,
            },
            //boolean
            new ExpressionTestCase<bool>()
            {
                Infix = "true",
                ExpectedExpression = () => true,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "false",
                ExpectedExpression = () => false,
            },
            //identifier
            new ExpressionTestCase()
            {
                Method = "bool main(bool BoolA) => BoolA;",
                Paramaters = new object[]
                {
                    true,
                },
                ExpectedResult = true,
            },
            new ExpressionTestCase()
            {
                Method = "int main(int IntA) => IntA;",
                 Paramaters = new object[]
                {
                    1,
                },
                ExpectedResult = 1,
            },
            new ExpressionTestCase()
            {
                Method = "double main(double DoubleA) => DoubleA;",
                Paramaters = new object[]
                {
                    1.5,
                },
                ExpectedResult = 1.5,
            },
            new ExpressionTestCase()
            {
                Method = "bool main(bool BoolA) { return BoolA; }",
                Paramaters = new object[]
                {
                    true,
                },
                ExpectedResult = true,
            },
            new ExpressionTestCase()
            {
                Method = "int main(int IntA) { return IntA; }",
                 Paramaters = new object[]
                {
                    1,
                },
                ExpectedResult = 1,
            },
            new ExpressionTestCase()
            {
                Method = "double main(double DoubleA) { return DoubleA; }",
                Paramaters = new object[]
                {
                    1.5,
                },
                ExpectedResult = 1.5,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
