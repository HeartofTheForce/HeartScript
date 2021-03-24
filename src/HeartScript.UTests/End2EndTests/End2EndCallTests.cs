using System;
using HeartScript;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable IDE0047

namespace HeartScript.UTests.End2EndTests
{
    [TestFixture]
    public class End2EndCallTests
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
            //Call
            new End2EndTestCase<double>()
            {
                Infix = "sin(a) + cos(b)",
                ExpectedNodeString = "(+ ($ sin a) ($ cos b))",
                ExpectedFunction = (Context<double> ctx) => Math.Sin(ctx.A) + Math.Cos(ctx.B),
            },
            //CallExpressionParameter
            new End2EndTestCase<double>()
            {
                Infix = "tan(b + c)",
                ExpectedNodeString = "($ tan (+ b c))",
                ExpectedFunction = (Context<double> ctx) => Math.Tan(ctx.B + ctx.C),
            },
            //CallChained
            new End2EndTestCase<double>()
            {
                Infix = "sin(cos(b))",
                ExpectedNodeString = "($ sin ($ cos b))",
                ExpectedFunction = (Context<double> ctx) => Math.Sin(Math.Cos(ctx.B)),
            },
            //CallMultiParameter
            new End2EndTestCase<double>()
            {
                Infix = "max(a,b) + min(b,c)",
                ExpectedNodeString = "(+ ($ max a b) ($ min b c))",
                ExpectedFunction = (Context<double> ctx) => Math.Max(ctx.A, ctx.B) + Math.Min(ctx.B, ctx.C),
            },
            //CallMultiExpressionParameter
            new End2EndTestCase<double>()
            {
                Infix = "max(a + b, b + c)",
                ExpectedNodeString = "($ max (+ a b) (+ b c))",
                ExpectedFunction = (Context<double> ctx) => Math.Max(ctx.A + ctx.B, ctx.B + ctx.C),
            },
            //CallNestedMultiExpressionParameter
            new End2EndTestCase<double>()
            {
                Infix = "max((a + b) * 2, (b / c))",
                ExpectedNodeString = "($ max (* (+ a b) 2) (/ b c))",
                ExpectedFunction = (Context<double> ctx) => Math.Max((ctx.A + ctx.B) * 2, (ctx.B / ctx.C)),
            },
            //CallChainedMultiParameter
            new End2EndTestCase<double>()
            {
                Infix = "max(min(c, b), max(c, a))",
                ExpectedNodeString = "($ max ($ min c b) ($ max c a))",
                ExpectedFunction = (Context<double> ctx) => Math.Max(Math.Min(ctx.C, ctx.B), Math.Max(ctx.C, ctx.A)),
            },
            //CallChainedMultiParameterUnary
            new End2EndTestCase<double>()
            {
                Infix = "max(min(c, b), -a)",
                ExpectedNodeString = "($ max ($ min c b) (- a))",
                ExpectedFunction = (Context<double> ctx) => Math.Max(Math.Min(ctx.C, ctx.B), -ctx.A),
            },
            //CallNestedExpressionParameter
            new End2EndTestCase<double>()
            {
                Infix = "sin(min(c, b) - a)",
                ExpectedNodeString = "($ sin (- ($ min c b) a))",
                ExpectedFunction = (Context<double> ctx) => Math.Sin(Math.Min(ctx.C, ctx.B) - ctx.A),
            },
            //PostfixInfixUnary
            new End2EndTestCase<double>()
            {
                Infix = "clamp(a,b,c) + - d",
                ExpectedNodeString = "(+ ($ clamp a b c) (- d))",
                ExpectedFunction = (Context<double> ctx) => Math.Clamp(ctx.A, ctx.B, ctx.C) + -ctx.D,
            },
            //Postfix
            new End2EndTestCase<double>()
            {
                Infix = "clamp(a + b, b - c, c * d)",
                ExpectedNodeString = "($ clamp (+ a b) (- b c) (* c d))",
                ExpectedFunction = (Context<double> ctx) => Math.Clamp(ctx.A + ctx.B, ctx.B - ctx.C, ctx.C * ctx.D),
            },
            //UnaryCall
            new End2EndTestCase<double>()
            {
                Infix = "-max(2, b)",
                ExpectedNodeString = "(- ($ max 2 b))",
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
