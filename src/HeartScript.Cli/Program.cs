﻿using System;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var operatorPatterns = OperatorPatternBuilder.Parse("./src/test.ops");

                string infix = string.Join(' ', args);

                Console.WriteLine("Input");
                Console.WriteLine(infix);

                var ctx = new ParserContext(infix);
                var node = ExpressionPattern.Parse(operatorPatterns, ctx);

                Console.WriteLine("Output");
                Console.WriteLine(node);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
