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
            var lexer = new Lexer(input);

            var parserCtx = new ParserContext();
            parserCtx.Patterns["term"] = ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(TerminalPattern.Create(s_regex))
                        .Or(TerminalPattern.Create(s_plainText)))
                    .Or(SequencePattern.Create()
                        .Then(TerminalPattern.Create(LexerPattern.FromPlainText("(")))
                        .Then(KeyPattern.Create("choice"))
                        .Then(TerminalPattern.Create(LexerPattern.FromPlainText(")"))))
                    .Or(TerminalPattern.Create(LexerPattern.FromRegex("\\w+")));

            parserCtx.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
                KeyPattern.Create("quantifier")
            );

            parserCtx.Patterns["quantifier"] = SequencePattern.Create()
                .Then(KeyPattern.Create("term"))
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(TerminalPattern.Create(LexerPattern.FromPlainText("?")))
                        .Or(TerminalPattern.Create(LexerPattern.FromPlainText("*")))
                        .Or(TerminalPattern.Create(LexerPattern.FromPlainText("+")))));

            parserCtx.Patterns["choice"] = SequencePattern.Create()
                .Then(KeyPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                        0,
                        SequencePattern.Create()
                            .Then(TerminalPattern.Create(LexerPattern.FromPlainText("/")))
                            .Then(KeyPattern.Create("sequence"))));

            var builderPattern = KeyPattern.Create("choice");
            var result = parserCtx.TryMatch(builderPattern, lexer);

            var builderCtx = OperatorInfoPegBuilder.CreateBuilder();
            var parserPattern = builderCtx.BuildKeyPattern(result.Value);

            return parserPattern;
        }

        public static void Test(Lexer lexer)
        {
            var ctx = new ParserContext();
            ctx.Patterns["expr"] = TerminalPattern.Create(LexerPattern.FromRegex("\\w+"));

            var pattern = BuildParser();
            var result = ctx.TryMatch(pattern, lexer);
        }
    }
}
