using System.IO;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    static class Peg
    {
        static IPattern BuildPattern()
        {
            var parser = OperatorInfoPegBuilder.CreateParser();

            string input = File.ReadAllText("src/peg.ops");
            var ctx = new ParserContext(input);
            var result = parser.TryMatch(ctx);

            var builderCtx = OperatorInfoPegBuilder.CreateBuilder();
            var parserPattern = builderCtx.BuildKeyPattern(result.Value.Children[1]);

            return parserPattern;
        }

        public static void Test(string input)
        {
            var ctx = new ParserContext(input);

            var pattern = BuildPattern().TrimLeft();
            var parser = new Parser(pattern);
            parser.Patterns["expr"] = LexerPattern.FromRegex("\\w+").TrimRight();

            var result = parser.TryMatch(ctx);
        }
    }
}
