using System;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionTests
{
    [TestFixture]
    public class PostfixTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //$
            new ExpressionTestCase<double>()
            {
                Infix = "Math.Sin(1)",
                ExpectedExpression = () => Math.Sin(1),
            },
            new ExpressionTestCase<int>()
            {
                Infix = "Math.Max(0, 1)",
                ExpectedExpression = () => Math.Max(0, 1),
            },
            new ExpressionTestCase<double>()
            {
                Infix = "Math.Max(0.5, 1.5)",
                ExpectedExpression = () => Math.Max(0.5, 1.5),
            },
            new ExpressionTestCase<double>()
            {
                Infix = "Math.Max(0.5, 1)",
                ExpectedExpression = () => Math.Max(0.5, 1),
            },
            new ExpressionTestCase<double>()
            {
                Infix = "Math.Max(0, 1.5)",
                ExpectedExpression = () => Math.Max(0, 1.5),
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
