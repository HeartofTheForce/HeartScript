using System;
using NUnit.Framework;

namespace HeartScript.Tests.CompilerTests
{
    [TestFixture]
    public class CompilerPostfixTests
    {
        private static readonly IExpressionCompilerTestCase[] s_testCases = new IExpressionCompilerTestCase[]
        {
            //$
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "Sin(1)",
                ExpectedExpression = () => Math.Sin(1),
            },
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "Max(0, 1)",
                ExpectedExpression = () => Math.Max(0, 1),
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "Max(0.5, 1.5)",
                ExpectedExpression = () => Math.Max(0.5, 1.5),
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "Max(0.5, 1)",
                ExpectedExpression = () => Math.Max(0.5, 1),
            },
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "Max(0, 1.5)",
                ExpectedExpression = () => Math.Max(0, 1.5),
            },
            //!
            new ExpressionCompilerTestCase<double>()
            {
                Infix = "1.5!",
                ExpectedExpression = () => 1.5,
            },
            new ExpressionCompilerTestCase<int>()
            {
                Infix = "1!",
                ExpectedExpression = () => 1,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionCompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
