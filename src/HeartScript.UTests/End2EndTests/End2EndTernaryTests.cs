using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable IDE0047

namespace HeartScript.UTests.End2EndTests
{
    [TestFixture]
    public class End2EndTernaryTests
    {
        static readonly Context<bool> s_ctx = new Context<bool>()
        {
            A = true,
            B = false,
            C = true,
            D = false,
            E = true,
            F = false,
            G = true,
            H = false,
            I = true,
        };

        static readonly End2EndTestCase<bool>[] s_testCases = new End2EndTestCase<bool>[]
        {
            //ChainedTernary
            new End2EndTestCase<bool>()
            {
                Infix = "a ? b : c ? d : e",
                ExpectedNodeString = "(? a b (? c d e))",
                ExpectedFunction = (Context<bool> ctx) => ctx.A ? ctx.B : ctx.C ? ctx.D : ctx.E,
            },
            //TernaryBrackets
            new End2EndTestCase<bool>()
            {
                Infix = "(a ? b : c) ? d : e",
                ExpectedNodeString = "(? (? a b c) d e)",
                ExpectedFunction = (Context<bool> ctx) => (ctx.A ? ctx.B : ctx.C) ? ctx.D : ctx.E,
            },
            //TernaryBinary
            new End2EndTestCase<bool>()
            {
                Infix = "a ? b : c + 1",
                ExpectedNodeString = "(? a b (+ c 1))",
                ExpectedFunction = (Context<bool> ctx) => (ctx.A ? ctx.B : ctx.C) ? ctx.D : ctx.E,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase<bool> testCase)
        {
            var lexer = new Lexer(testCase.Infix);

            var node = AstParser.Parse(Helpers.OperatorInfoBuilder.TestOperators, lexer);
            Assert.AreEqual(testCase.ExpectedNodeString, node.ToString());

            // var functionActual = ExpressionCompiler.Compile<Context<bool>, double>(Demo.CompilerFunctions, node);
            // Assert.AreEqual(testCase.ExpectedFunction(s_ctx), functionActual(s_ctx));
        }
    }
}
