using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class PegBuilderContext
    {
        public Dictionary<string, Func<PegBuilderContext, INode, IPattern>> Builders { get; }

        public PegBuilderContext()
        {
            Builders = new Dictionary<string, Func<PegBuilderContext, INode, IPattern>>();
        }

        public IPattern BuildKeyPattern(INode node)
        {
            var keyNode = (KeyNode)node;
            return Builders[keyNode.Key](this, keyNode.KeyValue);
        }
    }

    public static class PegBuilder
    {
        private static readonly LexerPattern s_regex = LexerPattern.FromRegex("`(?:``|[^`])*`");
        private static readonly LexerPattern s_plainText = LexerPattern.FromRegex("'(?:''|[^'])*'");
        private static readonly LexerPattern s_nonSignificant = LexerPattern.FromRegex("\\s+");

        public static IPattern TrimLeft(this IPattern pattern)
        {
            return SequencePattern.Create()
                .Then(QuantifierPattern.Optional(s_nonSignificant))
                .Then(pattern);
        }

        public static IPattern TrimRight(this IPattern pattern)
        {
            return SequencePattern.Create()
                .Then(pattern)
                .Then(QuantifierPattern.Optional(s_nonSignificant));
        }

        public static PatternParser CreateParser()
        {
            var parser = new PatternParser();

            parser.Patterns["term"] = ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(s_regex.TrimRight())
                        .Or(s_plainText.TrimRight()))
                    .Or(SequencePattern.Create()
                        .Then(LexerPattern.FromPlainText("(").TrimRight())
                        .Then(KeyPattern.Create("choice"))
                        .Then(LexerPattern.FromPlainText(")").TrimRight()))
                    .Or(LexerPattern.FromRegex("\\w+").TrimRight());

            parser.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
                KeyPattern.Create("quantifier")
            );

            parser.Patterns["quantifier"] = SequencePattern.Create()
                .Then(KeyPattern.Create("term"))
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(LexerPattern.FromPlainText("?").TrimRight())
                        .Or(LexerPattern.FromPlainText("*").TrimRight())
                        .Or(LexerPattern.FromPlainText("+").TrimRight())));

            parser.Patterns["choice"] = SequencePattern.Create()
                .Then(KeyPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                        0,
                        SequencePattern.Create()
                            .Then(LexerPattern.FromPlainText("/").TrimRight())
                            .Then(KeyPattern.Create("sequence"))));

            return parser;
        }

        public static PegBuilderContext CreateBuilder()
        {
            var output = new PegBuilderContext();

            output.Builders["sequence"] = BuildSequence;
            output.Builders["choice"] = BuildChoice;
            output.Builders["term"] = BuildTerm;
            output.Builders["quantifier"] = BuildQuantifier;

            return output;
        }

        static IPattern BuildChoice(PegBuilderContext ctx, INode node)
        {
            var minOrMoreNode = node.Children[1];

            if (minOrMoreNode.Children.Count == 0)
                return ctx.BuildKeyPattern(node.Children[0]);
            else
            {
                var output = ChoicePattern.Create()
                   .Or(ctx.BuildKeyPattern(node.Children[0]));

                foreach (var child in minOrMoreNode.Children)
                {
                    output.Or(ctx.BuildKeyPattern(child.Children[0]));
                }

                return output;
            }
        }

        static IPattern BuildSequence(PegBuilderContext ctx, INode node)
        {
            var output = SequencePattern.Create();
            foreach (var child in node.Children)
            {
                output.Then(ctx.BuildKeyPattern(child));
            }

            return output;
        }

        static IPattern BuildQuantifier(PegBuilderContext ctx, INode node)
        {
            var pattern = ctx.BuildKeyPattern(node.Children[0]);
            var optional = node.Children[1];

            if (optional.Children.Count == 0)
                return pattern;

            var choice = (ChoiceNode)optional.Children[0];
            return choice.ChoiceIndex switch
            {
                0 => QuantifierPattern.Optional(pattern),
                1 => QuantifierPattern.MinOrMore(0, pattern),
                2 => QuantifierPattern.MinOrMore(1, pattern),
                _ => throw new Exception(),
            };
        }

        static IPattern BuildTerm(PegBuilderContext ctx, INode node)
        {
            var root = (ChoiceNode)node;

            switch (root.ChoiceIndex)
            {
                case 0:
                    {
                        var choiceNode = (ChoiceNode)root.ChoiceValue;
                        var terminalNode = choiceNode.ChoiceValue.Children[0];

                        switch (choiceNode.ChoiceIndex)
                        {
                            case 0:
                                {
                                    string? pattern = terminalNode.Value[1..^1].Replace("``", "`");
                                    return LexerPattern.FromRegex(pattern).TrimRight();
                                }
                            case 1:
                                {
                                    string? pattern = terminalNode.Value[1..^1].Replace("''", "'");
                                    return LexerPattern.FromPlainText(pattern).TrimRight();
                                }
                            default: throw new Exception();
                        }
                    };
                case 1:
                    {
                        var sequenceNode = root.ChoiceValue;
                        return ctx.BuildKeyPattern(sequenceNode.Children[1]);
                    }
                case 2:
                    {
                        var terminalNode = root.ChoiceValue.Children[0];
                        return KeyPattern.Create(terminalNode.Value);
                    }
                default: throw new Exception();
            }
        }
    }
}
