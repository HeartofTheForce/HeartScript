using System;
using HeartScript.Compiling.Emit;
using Heart.Parsing;
using Heart.Parsing.Patterns;
using NUnit.Framework;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Tests
{
    public interface ICompilerTestCase
    {
        void Execute();
    }

    public class CompilerTestCase : ICompilerTestCase
    {
        public string Method { get; set; }
        public object[] Paramaters { get; set; }
        public object? ExpectedResult { get; set; }

        public void Execute()
        {
            var ctx = new ParserContext(Method);
            var pattern = Utility.Parser.Patterns["root"].Trim(Utility.Parser.Patterns["_"]);
            var node = pattern.MatchComplete(Utility.Parser, ctx);

            var compiledMethodInfo = EmitCompiler.CompileFunction(node);

            object? actualResult = compiledMethodInfo.Invoke(null, Paramaters);
            Assert.AreEqual(ExpectedResult, actualResult);
        }

        public override string ToString()
        {
            return $"\"{Method}\"";
        }
    }

    public class CompilerExceptionTestCase<T> : ICompilerTestCase
        where T : Exception
    {
        public string Method { get; set; }
        public string Message { get; set; }

        public void Execute()
        {
            var ctx = new ParserContext(Method);
            var pattern = Utility.Parser.Patterns["root"].Trim(Utility.Parser.Patterns["_"]);
            var node = pattern.MatchComplete(Utility.Parser, ctx);

            var exception = Assert.Throws<T>(() => EmitCompiler.CompileFunction(node));
            Assert.AreEqual(Message, exception.Message);
        }

        public override string ToString()
        {
            return $"\"{Method}\"";
        }
    }
}
