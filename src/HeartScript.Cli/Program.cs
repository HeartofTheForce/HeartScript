using System;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            string infix = string.Join(' ', args);
            Console.WriteLine(infix);

            var tokens = Lexer.Process(infix);
            var node = AstParser.Parse(Demo.Operators, tokens);
        }
    }
}
