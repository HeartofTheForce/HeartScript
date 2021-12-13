using NUnit.Framework;

namespace HeartScript.Tests.CompilerTests
{
    [TestFixture]
    public class CompilerInfixTests
    {
        private static readonly IExpressionCompilerTestCase[] s_testCases = new IExpressionCompilerTestCase[]
        {
            //*
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "2 * 3",
                ExpectedExpression = () => 2 * 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 * 3.5",
                ExpectedExpression = () => 2.5 * 3.5,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 * 3",
                ExpectedExpression = () => 2.5 * 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2 * 3.5",
                ExpectedExpression = () => 2 * 3.5,
            },
            ///
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "2 / 3",
                ExpectedExpression = () => 2 / 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 / 3.5",
                ExpectedExpression = () => 2.5 / 3.5,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 / 3",
                ExpectedExpression = () => 2.5 / 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2 / 3.5",
                ExpectedExpression = () => 2 / 3.5,
            },
            //+
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "2 + 3",
                ExpectedExpression = () => 2 + 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 + 3.5",
                ExpectedExpression = () => 2.5 + 3.5,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 + 3",
                ExpectedExpression = () => 2.5 + 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2 + 3.5",
                ExpectedExpression = () => 2 + 3.5,
            },
            //-
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "2 - 3",
                ExpectedExpression = () => 2 - 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 - 3.5",
                ExpectedExpression = () => 2.5 - 3.5,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2.5 - 3",
                ExpectedExpression = () => 2.5 - 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "2 - 3.5",
                ExpectedExpression = () => 2 - 3.5,
            },
            //<=
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 <= 2",
                ExpectedExpression = () => 2 <= 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 <= 2.5",
                ExpectedExpression = () => 2.5 <= 2.5,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 <= 2",
                ExpectedExpression = () => 2.5 <= 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 <= 2.5",
                ExpectedExpression = () => 2 <= 2.5,
            },
            //>=
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 >= 2",
                ExpectedExpression = () => 2 >= 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 >= 2.5",
                ExpectedExpression = () => 2.5 >= 2.5,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 >= 2",
                ExpectedExpression = () => 2.5 >= 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 >= 2.5",
                ExpectedExpression = () => 2 >= 2.5,
            },
            //<
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 < 2",
                ExpectedExpression = () => 2 < 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 < 2.5",
                ExpectedExpression = () => 2.5 < 2.5,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 < 2",
                ExpectedExpression = () => 2.5 < 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 < 2.5",
                ExpectedExpression = () => 2 < 2.5,
            },
            //>
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 > 2",
                ExpectedExpression = () => 2 > 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 > 2.5",
                ExpectedExpression = () => 2.5 > 2.5,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 > 2",
                ExpectedExpression = () => 2.5 > 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 > 2.5",
                ExpectedExpression = () => 2 > 2.5,
            },
            //==
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 == 2",
                ExpectedExpression = () => 2 == 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 == 2.5",
                ExpectedExpression = () => 2.5 == 2.5,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 == 2",
                ExpectedExpression = () => 2.5 == 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 == 2.5",
                ExpectedExpression = () => 2 == 2.5,
            },
            //!=
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 != 2",
                ExpectedExpression = () => 2 != 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 != 2.5",
                ExpectedExpression = () => 2.5 != 2.5,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2.5 != 2",
                ExpectedExpression = () => 2.5 != 2,
            },
            new ExpressionCompilerTestCase<bool>()
            {
                Infix = "2 != 2.5",
                ExpectedExpression = () => 2 != 2.5,
            },
            //&
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "2 & 3",
                ExpectedExpression = () => 2 & 3,
            },
            //^
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "2 ^ 3",
                ExpectedExpression = () => 2 ^ 3,
            },
            //|
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "2 | 3",
                ExpectedExpression = () => 2 | 3,
            },
            //?:
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "true ? 2 : 3",
                ExpectedExpression = () => true ? 2 : 3,
            },
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "false ? 2 : 3",
                ExpectedExpression = () => false ? 2 : 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "false ? 2.5 : 3.5",
                ExpectedExpression = () => false ? 2.5 : 3.5,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "false ? 2.5 : 3",
                ExpectedExpression = () => false ? 2.5 : 3,
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "false ? 2 : 3.5",
                ExpectedExpression = () => false ? 2 : 3.5,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionCompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
