using System;
using HeartScript;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable IDE0047

namespace HeartScript.UTests.End2EndTests
{
    [TestFixture]
    public class End2EndFloat
    {
        static readonly Context<double> s_ctx = new Context<double>()
        {
            A = 1.1,
            B = 2.2,
            C = 3.3,
            D = 4.4,
            E = 5.5,
            F = 6.6,
            G = 7.7,
            H = 8.8,
            I = 9.9,
        };

        static readonly End2EndTestCase<double>[] s_testCases = new End2EndTestCase<double>[]
        {
            //LeftToFloat
            new End2EndTestCase<double>()
            {
                Infix = "2 + 1.5",
                ExpectedNodeString = "(+ 2 1.5)",
                ExpectedFunction = (Context<double> ctx) => 2 + 1.5,
            },
            //RightToFloat
            new End2EndTestCase<double>()
            {
                Infix = "1.5 + 2",
                ExpectedNodeString = "(+ 1.5 2)",
                ExpectedFunction = (Context<double> ctx) => 1.5 + 2,
            },
            //SinCosTan
            new End2EndTestCase<double>()
            {
                Infix = "sin(1.0) + cos(1.0) + tan(1.0)",
                ExpectedNodeString = "(+ (+ (sin 1.0) (cos 1.0)) (tan 1.0))",
                ExpectedFunction = (Context<double> ctx) => Math.Sin(1.0) + Math.Cos(1.0) + Math.Tan(1.0),
            },
            //MaxIntFloat
            new End2EndTestCase<double>()
            {
                Infix = "max(2, b)",
                ExpectedNodeString = "(max 2 b)",
                ExpectedFunction = (Context<double> ctx) => Math.Max(2, ctx.B),
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase<double> testCase)
        {
            var tokens = Lexer.Process(testCase.Infix);

            var node = AstParser.Parse(Demo.Operators, tokens);
            Assert.AreEqual(testCase.ExpectedNodeString, node.ToString());

            // var functionActual = ExpressionCompiler.Compile<Context<double>, double>(Demo.CompilerFunctions, node);
            // Assert.AreEqual(testCase.ExpectedFunction(s_ctx), functionActual(s_ctx));
        }
    }
}
