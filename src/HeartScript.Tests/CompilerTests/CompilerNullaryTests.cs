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
            new ExpressionCompilerTestCase<DemoContext, bool>()
            {
                Infix = "BoolA",
                Input = new DemoContext()
                {
                    BoolA = true,
                },
                ExpectedString = "BoolA",
                ExpectedExpression = (ctx) => ctx.BoolA,
            },
            new ExpressionCompilerTestCase<DemoContext, int>()
            {
                Infix = "IntA",
                Input = new DemoContext()
                {
                    IntA = 1,
                },
                ExpectedString = "IntA",
                ExpectedExpression = (ctx) => ctx.IntA,
            },
            new ExpressionCompilerTestCase<DemoContext, double>()
            {
                Infix = "DoubleA",
                Input = new DemoContext()
                {
                    DoubleA = 1.5,
                },
                ExpectedString = "DoubleA",
                ExpectedExpression = (ctx) => ctx.DoubleA,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionCompilerTestCase testCase)
        {
            testCase.Execute(Helper.Parser);
        }
    }
}
