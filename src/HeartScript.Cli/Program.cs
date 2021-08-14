using System;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var operatorInfos = OperatorInfoBuilder.Parse("./src/test.ops");

                string infix = string.Join(' ', args);

                Console.WriteLine("Input");
                Console.WriteLine(infix);

                var lexer = new Lexer(infix);
                var node = ExpressionParser.Parse(operatorInfos, lexer);

                Console.WriteLine("Output");
                Console.WriteLine(node);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
