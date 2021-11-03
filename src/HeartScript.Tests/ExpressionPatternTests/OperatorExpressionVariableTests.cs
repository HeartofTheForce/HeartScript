using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.ExpressionPatternTests
{
    [TestFixture]
    public class OperatorExpressionVariableTests
    {
        private static readonly PatternParser s_parser;

        static OperatorExpressionVariableTests()
        {
            s_parser = ParsingHelper.BuildPatternParser("./TestOperators/operator-expression-variable.peg");
        }

        static readonly IExpressionTestCase[] s_testCases = new IExpressionTestCase[]
        {
            //{}
            new ExpressionTestCase()
            {
                Infix = "{x, y, z}",
                ExpectedOutput = "({ x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "{}",
                ExpectedOutput = "({)",
            },
            new ExpressionTestCase()
            {
                Infix = "{x}",
                ExpectedOutput = "({ x)",
            },
            //[]
            new ExpressionTestCase()
            {
                Infix = "[x y z]",
                ExpectedOutput = "([ x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "[]",
                ExpectedOutput = "([)",
            },
            new ExpressionTestCase()
            {
                Infix = "[x]",
                ExpectedOutput = "([ x)",
            },
            //?:
            new ExpressionTestCase()
            {
                Infix = "? x : y : z",
                ExpectedOutput = "(? x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "?",
                ExpectedOutput = "(?)",
            },
            new ExpressionTestCase()
            {
                Infix = "? x",
                ExpectedOutput = "(? x)",
            },
            //|
            new ExpressionTestCase()
            {
                Infix = "| x y z",
                ExpectedOutput = "(| x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "|",
                ExpectedOutput = "(|)",
            },
            new ExpressionTestCase()
            {
                Infix = "| x",
                ExpectedOutput = "(| x)",
            },
            //&*
            new ExpressionTestCase()
            {
                Infix = "& x * y * z *",
                ExpectedOutput = "(& x y z)",
            },
            new ExpressionTestCase()
            {
                Infix = "& *",
                ExpectedOutput = "(&)",
            },
            new ExpressionTestCase()
            {
                Infix = "& x *",
                ExpectedOutput = "(& x)",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(IExpressionTestCase testCase)
        {
            testCase.Execute(s_parser);
        }
    }
}
