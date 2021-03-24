using System;
using HeartScript;
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
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase<bool> testCase)
        {
            var tokens = Lexer.Process(testCase.Infix);

            var node = AstParser.Parse(Demo.Operators, tokens);
            Assert.AreEqual(testCase.ExpectedNodeString, node.ToString());

            // var functionActual = ExpressionCompiler.Compile<Context<bool>, double>(Demo.CompilerFunctions, node);
            // Assert.AreEqual(testCase.ExpectedFunction(s_ctx), functionActual(s_ctx));
        }
    }
}
