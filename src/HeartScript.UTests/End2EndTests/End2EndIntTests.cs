using HeartScript;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable IDE0047

namespace HeartScript.UTests.End2EndTests
{
    [TestFixture]
    public class End2EndIntTests
    {
        static readonly Context<int> s_ctx = new Context<int>()
        {
            A = 1,
            B = 2,
            C = 3,
            D = 4,
            E = 5,
            F = 6,
            G = 7,
            H = 8,
            I = 9,
        };

        static readonly End2EndTestCase<int>[] s_testCases = new End2EndTestCase<int>[]
        {
            //ReturnFloat2Int
            new End2EndTestCase<int>()
            {
                Infix = "2.5 + 3.3",
                ExpectedNodeString = "(+ 2.5 3.3)",
                ExpectedFunction = (Context<int> ctx) => (int)(2.5 + 3.3),
            },
            //IntOnlyBitwise
            new End2EndTestCase<int>()
            {
                Infix = "~(1 | 4)",
                ExpectedNodeString = "(~ (| 1 4))",
                ExpectedFunction = (Context<int> ctx) => ~(1 | 4),
            },
            //BinaryPostfix
            new End2EndTestCase<int>()
            {
                Infix = "2 * 1!",
                ExpectedNodeString = "(* 2 (! 1))",
                ExpectedFunction = (Context<int> ctx) => throw new System.NotImplementedException(),
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase<int> testCase)
        {
            var tokens = Lexer.Process(testCase.Infix);

            var node = AstParser.Parse(Demo.Operators, tokens);
            Assert.AreEqual(testCase.ExpectedNodeString, node.ToString());

            // var functionActual = ExpressionCompiler.Compile<Context<int>, int>(Demo.CompilerFunctions, node);
            // Assert.AreEqual(testCase.ExpectedFunction(s_ctx), functionActual(s_ctx));
        }
    }
}
