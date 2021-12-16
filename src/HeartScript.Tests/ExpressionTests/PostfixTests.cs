using System;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionTests
{
    [TestFixture]
    public class PostfixTests
    {
        private static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //$
            new ExpressionTestCase<double>()
            {
                Infix = "Sin(1)",
                ExpectedExpression = () => Math.Sin(1),
            },
            new ExpressionTestCase<int>()
            {
                Infix = "Max(0, 1)",
                ExpectedExpression = () => Math.Max(0, 1),
            },
            new ExpressionTestCase<double>()
            {
                Infix = "Max(0.5, 1.5)",
                ExpectedExpression = () => Math.Max(0.5, 1.5),
            },
            new ExpressionTestCase<double>()
            {
                Infix = "Max(0.5, 1)",
                ExpectedExpression = () => Math.Max(0.5, 1),
            },
            new ExpressionTestCase<double>()
            {
                Infix = "Max(0, 1.5)",
                ExpectedExpression = () => Math.Max(0, 1.5),
            },
            //!
            new ExpressionTestCase<double>()
            {
                Infix = "1.5!",
                ExpectedExpression = () => 1.5,
            },
            new ExpressionTestCase<int>()
            {
                Infix = "1!",
                ExpectedExpression = () => 1,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
