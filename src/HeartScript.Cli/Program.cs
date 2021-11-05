using System;
using HeartScript.Compiling;
using HeartScript.Parsing;
using HeartScript.Peg;

namespace HeartScript.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string infix = string.Join(' ', args);

                Console.WriteLine("Input");
                Console.WriteLine(infix);

                var ctx = new ParserContext(infix);
                var parser = PegHelper.BuildPatternParser("./src/test.peg");
                var node = parser.Patterns["root"].TryMatch(parser, ctx);

                ctx.AssertComplete();
                if (node == null)
                    throw new ArgumentException(nameof(ctx.Exception));

                Console.WriteLine("Output");
                Console.WriteLine(StringCompiler.Compile(node));

                object[]? parameters = new object[]
                {
                    1.1,
                    2.2,
                    3.3,
                };
                var compiledMethodInfo = EmitCompiler.CompileFunction(node);

                Console.WriteLine("Result");
                Console.WriteLine(compiledMethodInfo.Invoke(null, parameters));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
