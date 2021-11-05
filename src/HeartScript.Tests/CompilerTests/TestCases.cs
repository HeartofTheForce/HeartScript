using System;
using HeartScript.Compiling;
using HeartScript.Expressions;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Tests.CompilerTests
{
    public interface IExpressionCompilerTestCase
    {
        void Execute(PatternParser parser);
    }

    public class ExpressionCompilerTestCase<T> : IExpressionCompilerTestCase
    {
        public string Infix { get; set; }
        public string ExpectedString { get; set; }
        public Func<T> ExpectedExpression { get; set; }

        public void Execute(PatternParser parser)
        {
            var ctx = new ParserContext(Infix);

            var node = ExpressionPattern.Parse(parser, ctx);
            Assert.AreEqual(ExpectedString, node.ToString());

            var expectedResult = ExpectedExpression();
            var compiledExpression = EmitCompiler.CompileFunction<Func<T>>(node);
            var actualResult = compiledExpression();
            Assert.AreEqual(expectedResult, actualResult);
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }

    public class ExpressionCompilerTestCase : IExpressionCompilerTestCase
    {
        public string Method { get; set; }
        public object[] Paramaters { get; set; }
        public object? ExpectedResult { get; set; }

        public void Execute(PatternParser parser)
        {
            var ctx = new ParserContext(Method);
            var node = parser.Patterns["root"].TryMatch(parser, ctx);

            ctx.AssertComplete();
            if (node == null)
                throw new ArgumentException(nameof(ctx.Exception));

            var compiledMethodInfo = EmitCompiler.CompileFunction(node);
            object? actualResult = compiledMethodInfo.Invoke(null, Paramaters);

            Assert.AreEqual(ExpectedResult, actualResult);
        }

        public override string ToString()
        {
            return $"\"{Method}\"";
        }
    }
}
