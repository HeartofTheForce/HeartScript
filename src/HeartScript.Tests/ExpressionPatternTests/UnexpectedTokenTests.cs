using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class UnexpectedTokenTests
    {
        static readonly UnexpectedTokenTestCase[] s_testCases = new UnexpectedTokenTestCase[]
        {
            //Constant Constant
            new UnexpectedTokenTestCase()
            {
                Infix = "1 1",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //Identifier Constant
            new UnexpectedTokenTestCase()
            {
                Infix = "a 1",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //Constant Identifier
            new UnexpectedTokenTestCase()
            {
                Infix = "1 a",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //Identifier Identifier
            new UnexpectedTokenTestCase()
            {
                Infix = "a a",
                ExpectedCharIndex = 2,
                ExpectedPattern = "EOF",
            },
            //RoundClose Identifier
            new UnexpectedTokenTestCase()
            {
                Infix = "(1) a",
                ExpectedCharIndex = 4,
                ExpectedPattern = "EOF",
            },
            //RoundClose Constant
            new UnexpectedTokenTestCase()
            {
                Infix = "(a) 1",
                ExpectedCharIndex = 4,
                ExpectedPattern = "EOF",
            },
            //Unexpected Close
            new UnexpectedTokenTestCase()
            {
                Infix = "(1+2))",
                ExpectedCharIndex = 5,
                ExpectedPattern = "EOF",
            },
            //Bracket Missing Close
            new UnexpectedTokenTestCase()
            {
                Infix = "(1+2",
                ExpectedCharIndex = 4,
                ExpectedPattern = ")",
            },
            //Call Missing Close
            new UnexpectedTokenTestCase()
            {
                Infix = "max(1,2",
                ExpectedCharIndex = 7,
                ExpectedPattern = ")",
            },
            //Unexpected Comma
            new UnexpectedTokenTestCase()
            {
                Infix = "(1,)",
                ExpectedCharIndex = 2,
                ExpectedPattern = ")",
            },
            //Too Few Operands
            new UnexpectedTokenTestCase()
            {
                Infix = "a ? b",
                ExpectedCharIndex = 5,
                ExpectedPattern = ":",
            },
            //Empty Open Delimiter
            new UnexpectedTokenTestCase()
            {
                Infix = "max( , 2, 3)",
                ExpectedCharIndex = 5,
                ExpectedPattern = ")",
            },
            //Too Many Operands
            new UnexpectedTokenTestCase()
            {
                Infix = "a ? b : c : d",
                ExpectedCharIndex = 10,
                ExpectedPattern = "EOF",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(UnexpectedTokenTestCase testCase)
        {
            testCase.Execute(Helper.TestOperators);
        }
    }
}
