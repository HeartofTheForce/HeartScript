using NUnit.Framework;

namespace HeartScript.Tests.ExpressionTests
{
    [TestFixture]
    public class InfixTests
    {
        private static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //*
            new ExpressionTestCase<int>()
            {
                Infix = "2 * 3",
                ExpectedExpression = () => 2 * 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 * 3.5",
                ExpectedExpression = () => 2.5 * 3.5,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 * 3",
                ExpectedExpression = () => 2.5 * 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2 * 3.5",
                ExpectedExpression = () => 2 * 3.5,
            },
            ///
            new ExpressionTestCase<int>()
            {
                Infix = "2 / 3",
                ExpectedExpression = () => 2 / 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 / 3.5",
                ExpectedExpression = () => 2.5 / 3.5,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 / 3",
                ExpectedExpression = () => 2.5 / 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2 / 3.5",
                ExpectedExpression = () => 2 / 3.5,
            },
            //+
            new ExpressionTestCase<int>()
            {
                Infix = "2 + 3",
                ExpectedExpression = () => 2 + 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 + 3.5",
                ExpectedExpression = () => 2.5 + 3.5,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 + 3",
                ExpectedExpression = () => 2.5 + 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2 + 3.5",
                ExpectedExpression = () => 2 + 3.5,
            },
            //-
            new ExpressionTestCase<int>()
            {
                Infix = "2 - 3",
                ExpectedExpression = () => 2 - 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 - 3.5",
                ExpectedExpression = () => 2.5 - 3.5,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2.5 - 3",
                ExpectedExpression = () => 2.5 - 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "2 - 3.5",
                ExpectedExpression = () => 2 - 3.5,
            },
            //<=
            new ExpressionTestCase<bool>()
            {
                Infix = "2 <= 2",
                ExpectedExpression = () => 2 <= 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 <= 2.5",
                ExpectedExpression = () => 2.5 <= 2.5,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 <= 2",
                ExpectedExpression = () => 2.5 <= 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2 <= 2.5",
                ExpectedExpression = () => 2 <= 2.5,
            },
            //>=
            new ExpressionTestCase<bool>()
            {
                Infix = "2 >= 2",
                ExpectedExpression = () => 2 >= 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 >= 2.5",
                ExpectedExpression = () => 2.5 >= 2.5,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 >= 2",
                ExpectedExpression = () => 2.5 >= 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2 >= 2.5",
                ExpectedExpression = () => 2 >= 2.5,
            },
            //<
            new ExpressionTestCase<bool>()
            {
                Infix = "2 < 2",
                ExpectedExpression = () => 2 < 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 < 2.5",
                ExpectedExpression = () => 2.5 < 2.5,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 < 2",
                ExpectedExpression = () => 2.5 < 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2 < 2.5",
                ExpectedExpression = () => 2 < 2.5,
            },
            //>
            new ExpressionTestCase<bool>()
            {
                Infix = "2 > 2",
                ExpectedExpression = () => 2 > 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 > 2.5",
                ExpectedExpression = () => 2.5 > 2.5,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 > 2",
                ExpectedExpression = () => 2.5 > 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2 > 2.5",
                ExpectedExpression = () => 2 > 2.5,
            },
            //==
            new ExpressionTestCase<bool>()
            {
                Infix = "2 == 2",
                ExpectedExpression = () => 2 == 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 == 2.5",
                ExpectedExpression = () => 2.5 == 2.5,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 == 2",
                ExpectedExpression = () => 2.5 == 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2 == 2.5",
                ExpectedExpression = () => 2 == 2.5,
            },
            //!=
            new ExpressionTestCase<bool>()
            {
                Infix = "2 != 2",
                ExpectedExpression = () => 2 != 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 != 2.5",
                ExpectedExpression = () => 2.5 != 2.5,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2.5 != 2",
                ExpectedExpression = () => 2.5 != 2,
            },
            new ExpressionTestCase<bool>()
            {
                Infix = "2 != 2.5",
                ExpectedExpression = () => 2 != 2.5,
            },
            //&
            new ExpressionTestCase<int>()
            {
                Infix = "2 & 3",
                ExpectedExpression = () => 2 & 3,
            },
            //^
            new ExpressionTestCase<int>()
            {
                Infix = "2 ^ 3",
                ExpectedExpression = () => 2 ^ 3,
            },
            //|
            new ExpressionTestCase<int>()
            {
                Infix = "2 | 3",
                ExpectedExpression = () => 2 | 3,
            },
            //?:
            new ExpressionTestCase<int>()
            {
                Infix = "true ? 2 : 3",
                ExpectedExpression = () => true ? 2 : 3,
            },
            new ExpressionTestCase<int>()
            {
                Infix = "false ? 2 : 3",
                ExpectedExpression = () => false ? 2 : 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "false ? 2.5 : 3.5",
                ExpectedExpression = () => false ? 2.5 : 3.5,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "false ? 2.5 : 3",
                ExpectedExpression = () => false ? 2.5 : 3,
            },
            new ExpressionTestCase<double>()
            {
                Infix = "false ? 2 : 3.5",
                ExpectedExpression = () => false ? 2 : 3.5,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
