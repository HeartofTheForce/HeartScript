using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class ExpressionTermTests
    {
        static readonly ExpressionTermTestCase[] s_testCases = new ExpressionTermTestCase[]
        {
            //Empty Delimiter Close
            new ExpressionTermTestCase()
            {
                Infix = "max(1, 2, )",
                ExpectedTextOffset = 10,
            },
            //Empty Delimiter Delimiter
            new ExpressionTermTestCase()
            {
                Infix = "max(1, ,3)",
                ExpectedTextOffset = 7,
            },
            //TODO Empty Open Delimiter
            // new ExpressionTermTestCase()
            // {
            //     Infix = "max( , 2, 3)",
            //     ExpectedTextOffset = 5,
            // },
            //Empty Delimiter EndOfString
            new ExpressionTermTestCase()
            {
                Infix = "max(1, 2, ",
                ExpectedTextOffset = 10,
            },
            //Empty Brackets
            new ExpressionTermTestCase()
            {
                Infix = "()",
                ExpectedTextOffset = 1,
            },
            //Empty Binary Right
            new ExpressionTermTestCase()
            {
                Infix = "1 +",
                ExpectedTextOffset = 3,
            },
            //Empty Unary Right
            new ExpressionTermTestCase()
            {
                Infix = "-",
                ExpectedTextOffset = 1,
            },
            //Nested Empty Binary Right
            new ExpressionTermTestCase()
            {
                Infix = "(1 + ) 2",
                ExpectedTextOffset = 5,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ExpressionTermTestCase testCase)
        {
            testCase.Execute(Helper.Parser);
        }
    }
}
