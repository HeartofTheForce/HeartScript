using System;
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
            A = 2,
            B = 3,
            C = 5,
            D = 7,
            E = 11,
            F = 13,
            G = 17,
            H = 19,
            I = 23,
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
                ExpectedFunction = (Context<int> ctx) => throw new NotImplementedException(),
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase<int> testCase)
        {
            var lexer = new Lexer(testCase.Infix);

            var node = AstParser.Parse(Demo.Operators, lexer);
            Assert.AreEqual(testCase.ExpectedNodeString, node.ToString());

            // var functionActual = ExpressionCompiler.Compile<Context<int>, int>(Demo.CompilerFunctions, node);
            // Assert.AreEqual(testCase.ExpectedFunction(s_ctx), functionActual(s_ctx));
        }
    }
}
