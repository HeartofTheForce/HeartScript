using System;
using System.Linq.Expressions;
using HeartScript.Compiling;
using HeartScript.Expressions;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var operators = OperatorInfoBuilder.Parse("./src/test.ops");

                string infix = string.Join(' ', args);

                Console.WriteLine("Input");
                Console.WriteLine(infix);

                var ctx = new ParserContext(infix);
                var node = ExpressionPattern.Parse(operators, ctx);

                if (node == null)
                {
                    if (ctx.Exception != null)
                        throw ctx.Exception;
                    else
                        throw new Exception($"{infix} failed to parse");
                }

                Console.WriteLine("Output");
                Console.WriteLine(node);

                var compiledExpression = ExpressionNodeCompiler.Compile((ExpressionNode)node);
                var compiledFunction = Expression.Lambda<Func<double>>(compiledExpression).Compile();

                Console.WriteLine("Result");
                Console.WriteLine(compiledFunction());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
