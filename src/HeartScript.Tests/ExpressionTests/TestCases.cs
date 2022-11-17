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

            var pattern = Utility.Parser.Patterns["root"].Trim();
            var node = Utility.Parser.MatchComplete(pattern, source);

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
