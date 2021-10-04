using System;
using System.Collections.Generic;
using HeartScript.Nodes;
#pragma warning disable IDE0066

namespace HeartScript.Parsing
{
    public class PegBuilder
    {
        public Dictionary<string, Func<PegBuilder, INode, IPattern>> Builders { get; }

        public PegBuilder()
        {
            Builders = new Dictionary<string, Func<PegBuilder, INode, IPattern>>();
        }

        public IPattern BuildKeyPattern(KeyNode keyNode)
        {
            return Builders[keyNode.Key](this, keyNode.Node);
        }
    }

    public static class PegBuilderHelper
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

        public static PegBuilder CreateBuilder()
        {
            var output = new PegBuilder();

            output.Builders["sequence"] = BuildSequence;
            output.Builders["choice"] = BuildChoice;
            output.Builders["term"] = BuildTerm;
            output.Builders["quantifier"] = BuildQuantifier;

            return output;
        }

        static IPattern BuildChoice(PegBuilder ctx, INode node)
        {
            var minOrMoreNode = node.Children[1];

            if (minOrMoreNode.Children.Count == 0)
                return ctx.BuildKeyPattern((KeyNode)node.Children[0]);
            else
            {
                var output = ChoicePattern.Create()
                   .Or(ctx.BuildKeyPattern((KeyNode)node.Children[0]));

                foreach (var child in minOrMoreNode.Children)
                {
                    output.Or(ctx.BuildKeyPattern((KeyNode)child.Children[0]));
                }

                return output;
            }
        }

        static IPattern BuildSequence(PegBuilder ctx, INode node)
        {
            var output = SequencePattern.Create();
            foreach (var child in node.Children)
            {
                output.Then(ctx.BuildKeyPattern((KeyNode)child));
            }

            return output;
        }

        static IPattern BuildQuantifier(PegBuilder ctx, INode node)
        {
            var pattern = ctx.BuildKeyPattern((KeyNode)node.Children[0]);
            var optional = node.Children[1];

            if (optional.Children.Count == 0)
                return pattern;

            var choice = (ChoiceNode)optional.Children[0];
            switch (choice.ChoiceIndex)
            {
                case 0: return QuantifierPattern.Optional(pattern);
                case 1: return QuantifierPattern.MinOrMore(0, pattern);
                case 2: return QuantifierPattern.MinOrMore(1, pattern);
                default: throw new Exception();
            };
        }

        static IPattern BuildTerm(PegBuilder ctx, INode node)
        {
            var root = (ChoiceNode)node;

            switch (root.ChoiceIndex)
            {
                case 0:
                    {
                        var choiceNode = (ChoiceNode)root.Node;
                        var terminalNode = choiceNode.Node.Children[0];

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
                        var sequenceNode = root.Node;
                        return ctx.BuildKeyPattern((KeyNode)sequenceNode.Children[1]);
                    }
                case 2:
                    {
                        var terminalNode = root.Node.Children[0];
                        return KeyPattern.Create(terminalNode.Value);
                    }
                default: throw new Exception();
            }
        }
    }
}
