using System;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable IDE0047

namespace HeartScript.UTests.End2EndTests
{
    [TestFixture]
    public class End2EndIfElseTests
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
            //IfElseTernary
            new End2EndTestCase<bool>()
            {
                Infix = "if a ? b : c d ? e : f else g ? h : i",
                ExpectedNodeString = "(else (if (? a b c) (? d e f)) (? g h i))",
                ExpectedFunction = (Context<bool> ctx) => throw new NotImplementedException(),
            },
            //ChainedIfElse
            new End2EndTestCase<bool>()
            {
                Infix = "if a b else if c d else e",
                ExpectedNodeString = "(else (if a b) (else (if c d) e))",
                ExpectedFunction = (Context<bool> ctx) => throw new NotImplementedException(),
            },
            //If
            new End2EndTestCase<bool>()
            {
                Infix = "if a b",
                ExpectedNodeString = "(if a b)",
                ExpectedFunction = (Context<bool> ctx) => throw new NotImplementedException(),
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
