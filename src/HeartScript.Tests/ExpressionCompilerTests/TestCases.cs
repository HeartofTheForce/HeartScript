using System;
using System.Collections.Generic;
using HeartScript.Compiling;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Tests.CompilerTests
{
    public interface IExpressionCompilerTestCase
    {
        void Execute();
    }

    public class ExpressionCompilerTestCase<T> : IExpressionCompilerTestCase
    {
        private static Dictionary<Type, string> s_typeLookup = new()
        {
            [typeof(int)] = "int",
            [typeof(double)] = "double",
            [typeof(bool)] = "bool",
        };

        public string Infix { get; set; }
        public Func<T> ExpectedExpression { get; set; }

        public void Execute()
        {
            string source = $"{s_typeLookup[typeof(T)]} main() => {Infix};";
            var ctx = new ParserContext(source);

            var pattern = Helper.Parser.Patterns["root"];
            var node = pattern.TryMatch(Helper.Parser, ctx);

            ctx.AssertComplete();
            if (node == null)
                throw new ArgumentException(nameof(ctx.Exception));

            var compiledExpression = EmitCompiler.CompileFunction<Func<T>>(node);

            var expectedResult = ExpectedExpression();
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

        public void Execute()
        {
            var ctx = new ParserContext(Method);
            var pattern = Helper.Parser.Patterns["root"];
            var node = pattern.TryMatch(Helper.Parser, ctx);

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
