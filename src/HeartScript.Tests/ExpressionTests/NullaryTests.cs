using NUnit.Framework;

namespace HeartScript.Tests.ExpressionTests
{
    [TestFixture]
    public class NullaryTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
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
            new CompilerTestCase()
            {
                Method = "bool main(bool BoolA) => BoolA;",
                Paramaters = new object[]
                {
                    true,
                },
                ExpectedResult = true,
            },
            new CompilerTestCase()
            {
                Method = "int main(int IntA) => IntA;",
                 Paramaters = new object[]
                {
                    1,
                },
                ExpectedResult = 1,
            },
            new CompilerTestCase()
            {
                Method = "double main(double DoubleA) => DoubleA;",
                Paramaters = new object[]
                {
                    1.5,
                },
                ExpectedResult = 1.5,
            },
            new CompilerTestCase()
            {
                Method = "bool main(bool BoolA) { return BoolA; }",
                Paramaters = new object[]
                {
                    true,
                },
                ExpectedResult = true,
            },
            new CompilerTestCase()
            {
                Method = "int main(int IntA) { return IntA; }",
                 Paramaters = new object[]
                {
                    1,
                },
                ExpectedResult = 1,
            },
            new CompilerTestCase()
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
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
