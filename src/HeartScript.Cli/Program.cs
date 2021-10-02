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
                var operatorPatterns = OperatorPatternBuilder.Parse("./src/peg.ops");

                string infix = string.Join(' ', args);

                Console.WriteLine("Input");
                Console.WriteLine(infix);

                var ctx = new ParserContext(infix);
                var parser = new Parser();

                var expressionPattern = new ExpressionPattern(operatorPatterns);
                parser.Patterns["expr"] = expressionPattern;
                var result = parser.TryMatch(expressionPattern, ctx);

                // var lexer = new Lexer(infix);
                // var node = ExpressionParser.Parse(operatorInfos, lexer);

                // Console.WriteLine("Output");
                // Console.WriteLine(node);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
