using System;
using HeartScript.Compiling.Emit;
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
            var pattern = Utility.Parser.Patterns["root"].Trim();
            var node = Utility.Parser.MatchComplete(pattern, Method);

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
            var pattern = Utility.Parser.Patterns["root"].Trim();
            var node = Utility.Parser.MatchComplete(pattern, Method);

            var ex = Assert.Throws<T>(() => EmitCompiler.CompileFunction(node));
            Assert.AreEqual(Message, ex?.Message);
        }

        public override string ToString()
        {
            return $"\"{Method}\"";
        }
    }
}
