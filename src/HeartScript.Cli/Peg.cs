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

            var pattern = BuildPattern();
            var parser = new Parser(pattern);
            parser.Patterns["expr"] = SequencePattern.Create()
                .Then(QuantifierPattern.Optional(LexerPattern.FromRegex("\\s+")))
                .Then(LexerPattern.FromRegex("\\w+"));

            var result = parser.TryMatch(ctx);
        }
    }
}
