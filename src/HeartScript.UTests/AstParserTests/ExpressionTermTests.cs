using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.AstParserTests
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
                ExpectedCharIndex = 10,
            },
            //Empty Delimiter Delimiter
            new ExpressionTermTestCase()
            {
                Infix = "max(1, ,3)",
                ExpectedCharIndex = 7,
            },
            //Empty Open Delimiter
            new ExpressionTermTestCase()
            {
                Infix = "max( , 2, 3)",
                ExpectedCharIndex = 5,
            },
            //Empty Delimiter EndOfString
            new ExpressionTermTestCase()
            {
                Infix = "max(1, 2, ",
                ExpectedCharIndex = 10,
            },
            //Empty Brackets
            new ExpressionTermTestCase()
            {
                Infix = "()",
                ExpectedCharIndex = 1,
            },
            //Empty Binary Right
            new ExpressionTermTestCase()
            {
                Infix = "1 +",
                ExpectedCharIndex = 3,
            },
            //Empty Unary Right
            new ExpressionTermTestCase()
            {
                Infix = "-",
                ExpectedCharIndex = 1,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ExpressionTermTestCase testCase)
        {
            var lexer = new Lexer(testCase.Infix);
            var ex = Assert.Throws<ExpressionTermException>(() => AstParser.Parse(Helper.TestOperators, lexer));

            Assert.AreEqual(testCase.ExpectedCharIndex, ex.CharIndex);
        }

        public struct ExpressionTermTestCase
        {
            public string Infix { get; set; }
            public int ExpectedCharIndex { get; set; }

            public override string ToString()
            {
                return $"\"{Infix}\"";
            }
        }
    }
}
