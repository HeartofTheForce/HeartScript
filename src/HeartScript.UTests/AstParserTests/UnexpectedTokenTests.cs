using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.UTests.AstParserTests
{
    [TestFixture]
    public class UnexpectedTokenTests
    {
        static readonly UnexpectedTokenIndexTestCase[] s_testCases = new UnexpectedTokenIndexTestCase[]
        {
            //Constant Constant
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "1 1",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //Identifier Constant
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "a 1",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //Constant Identifier
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "1 a",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //Identifier Identifier
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "a a",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //RoundClose Identifier
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "(1) a",
                ExpectedCharIndex = 4,
                ExpectedPattern = "EOF",
            },
            //RoundClose Constant
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "(a) 1",
                ExpectedCharIndex = 4,
                ExpectedPattern = "EOF",
            },
            //Unexpected Close
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "(1+2))",
                ExpectedCharIndex = 5,
                ExpectedPattern = "EOF",
            },
            //Bracket Missing Close
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "(1+2",
                ExpectedCharIndex = 4,
                ExpectedPattern = ")",
            },
            //Call Missing Close
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "max(1,2",
                ExpectedCharIndex = 7,
                ExpectedPattern = ")",
            },
            //Unexpected Comma
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "(1,)",
                ExpectedCharIndex = 2,
                ExpectedPattern = ")",
            },
            //Too Few Operands
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "a ? b",
                ExpectedCharIndex = 5,
                ExpectedPattern = ":",
            },
            //Too Many Operands
            new UnexpectedTokenIndexTestCase()
            {
                Infix = "a ? b : c : d",
                ExpectedCharIndex = 10,
                ExpectedPattern = "EOF",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(UnexpectedTokenIndexTestCase testCase)
        {
            var lexer = new Lexer(testCase.Infix);
            var ex = Assert.Throws<UnexpectedTokenException>(() => AstParser.Parse(Helpers.OperatorInfoBuilder.TestOperators, lexer));

            Assert.AreEqual(testCase.ExpectedCharIndex, ex.CharIndex);
            Assert.AreEqual(testCase.ExpectedPattern, ex.ExpectedPattern);
        }

        public struct UnexpectedTokenIndexTestCase
        {
            public string Infix { get; set; }
            public int ExpectedCharIndex { get; set; }
            public string ExpectedPattern { get; set; }

            public override string ToString()
            {
                return $"\"{Infix}\"";
            }
        }
    }
}
