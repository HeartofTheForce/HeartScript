using System.IO;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    static class Peg
    {
        private static readonly LexerPattern s_regex = LexerPattern.FromRegex("`(?:``|[^`])*`");
        private static readonly LexerPattern s_plainText = LexerPattern.FromRegex("'(?:''|[^'])*'");

        static IPattern BuildParser()
        {
            string input = File.ReadAllText("src/peg.ops");
            var ctx = new ParserContext(input);

            var parser = new Parser();
            parser.Patterns["term"] = ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(s_regex)
                        .Or(s_plainText))
                    .Or(SequencePattern.Create()
                        .Then(LexerPattern.FromPlainText("("))
                        .Then(KeyPattern.Create("choice"))
                        .Then(LexerPattern.FromPlainText(")")))
                    .Or(LexerPattern.FromRegex("\\w+"));

            parser.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
                KeyPattern.Create("quantifier")
            );

            parser.Patterns["quantifier"] = SequencePattern.Create()
                .Then(KeyPattern.Create("term"))
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(LexerPattern.FromPlainText("?"))
                        .Or(LexerPattern.FromPlainText("*"))
                        .Or(LexerPattern.FromPlainText("+"))));

            parser.Patterns["choice"] = SequencePattern.Create()
                .Then(KeyPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                        0,
                        SequencePattern.Create()
                            .Then(LexerPattern.FromPlainText("/"))
                            .Then(KeyPattern.Create("sequence"))));

            var builderPattern = KeyPattern.Create("choice");
            var result = parser.TryMatch(builderPattern, ctx);

            var builderCtx = OperatorInfoPegBuilder.CreateBuilder();
            var parserPattern = builderCtx.BuildKeyPattern(result.Value);

            return parserPattern;
        }

        public static void Test(string input)
        {
            var ctx = new ParserContext(input);

            var parser = new Parser();
            parser.Patterns["expr"] = LexerPattern.FromRegex("\\w+");

            var pattern = BuildParser();
            var result = parser.TryMatch(pattern, ctx);
        }
    }
}
