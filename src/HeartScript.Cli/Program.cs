using System;
using System.IO;
using HeartScript.Compiling.Emit;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Cli
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                string source = File.ReadAllText(args[0]);

                Console.WriteLine("Input");
                Console.WriteLine(source);

                var ctx = new ParserContext(source);
                var parser = ParsingHelper.BuildPatternParser("./src/test.hg");
                var pattern = parser.Patterns["root"].Trim(parser.Patterns["_"]);
                var node = pattern.TryMatch(parser, ctx);

                ctx.AssertComplete();
                if (node == null)
                    throw new ArgumentException(nameof(ctx.Exception));

                Console.WriteLine("Parsed");
                Console.WriteLine(StringCompiler.Compile(node));

                object[]? parameters = new object[]
                {
                    1.1,
                    2.2,
                    3.3,
                };
                var compiledMethodInfo = EmitCompiler.CompileFunction(node);

                Console.WriteLine("Output");
                Console.WriteLine(compiledMethodInfo.Invoke(null, parameters));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
