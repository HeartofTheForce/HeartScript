using System;
using System.Collections.Generic;
using HeartScript.Compiling.Emit;
using Heart.Parsing;
using Heart.Parsing.Patterns;
using NUnit.Framework;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Tests.ExpressionTests
{
    public class ExpressionTestCase<T> : ICompilerTestCase
    {
        private static readonly Dictionary<Type, string> s_typeLookup = new Dictionary<Type, string>()
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

            var pattern = Utility.Parser.Patterns["root"].Trim(Utility.Parser.Patterns["_"]);
            var node = pattern.TryMatch(Utility.Parser, ctx);

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
}
