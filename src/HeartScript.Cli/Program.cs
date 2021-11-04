using System;
using HeartScript.Compiling;
using HeartScript.Expressions;
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

                if (node == null)
                {
                    if (ctx.Exception != null)
                        throw ctx.Exception;
                    else
                        throw new Exception($"{infix} failed to parse");
                }

                Console.WriteLine("Output");
                Console.WriteLine(StringCompiler.Compile(node));

                var compiledFunction = EmitCompiler.CompileFunction<TestContext<double>, double>(node);
                var testContext = new TestContext<double>()
                {
                    A = 1.1,
                    B = 2.2,
                    C = 3.3,
                };

                Console.WriteLine("Result");
                Console.WriteLine(compiledFunction(testContext));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class TestContext<T>
        where T : struct
    {
        public T A { get; set; }
        public T B { get; set; }
        public T C { get; set; }
    }
}
