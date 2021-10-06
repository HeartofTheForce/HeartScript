using System;
using HeartScript.Parsing;
using HeartScript.Peg.Nodes;
using HeartScript.Peg.Patterns;
#pragma warning disable IDE0066

namespace HeartScript.Peg
{
    public static class PegHelper
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

        public static PatternParser CreatePegParser()
        {
            var parser = new PatternParser();

            parser.Patterns["peg"] = LookupPattern.Create("choice").TrimLeft();

            parser.Patterns["choice"] = SequencePattern.Create()
                .Then(LookupPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                        0,
                        SequencePattern.Create()
                            .Then(LexerPattern.FromPlainText("/").TrimRight())
                            .Then(LookupPattern.Create("sequence"))));

            parser.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
                LookupPattern.Create("quantifier"));

            parser.Patterns["quantifier"] = SequencePattern.Create()
                .Then(LookupPattern.Create("term"))
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(LexerPattern.FromPlainText("?").TrimRight())
                        .Or(LexerPattern.FromPlainText("*").TrimRight())
                        .Or(LexerPattern.FromPlainText("+").TrimRight())));

            parser.Patterns["term"] = ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(s_regex.TrimRight())
                        .Or(s_plainText.TrimRight()))
                    .Or(SequencePattern.Create()
                        .Then(LexerPattern.FromPlainText("(").TrimRight())
                        .Then(LookupPattern.Create("choice"))
                        .Then(LexerPattern.FromPlainText(")").TrimRight()))
                    .Or(LexerPattern.FromRegex("\\w+").TrimRight());

            return parser;
        }

        public static IPattern BuildPegPattern(INode node)
        {
            var output = new PatternBuilder();

            output.Builders["peg"] = BuildPeg;
            output.Builders["choice"] = BuildChoice;
            output.Builders["sequence"] = BuildSequence;
            output.Builders["quantifier"] = BuildQuantifier;
            output.Builders["term"] = BuildTerm;

            return BuildLookup(output, (LookupNode)node);
        }

        static IPattern BuildLookup(PatternBuilder builder, LookupNode node)
        {
            if (node.Name == null)
                throw new ArgumentException($"{nameof(node.Name)} cannot be null");

            return builder.Builders[node.Name](builder, node.Node);
        }

        static IPattern BuildPeg(PatternBuilder builder, INode node)
        {
            return BuildLookup(builder, (LookupNode)node.Children[1]);
        }

        static IPattern BuildChoice(PatternBuilder builder, INode node)
        {
            var minOrMoreNode = node.Children[1];

            if (minOrMoreNode.Children.Count == 0)
                return BuildLookup(builder, (LookupNode)node.Children[0]);
            else
            {
                var output = ChoicePattern.Create()
                   .Or(BuildLookup(builder, (LookupNode)node.Children[0]));

                foreach (var child in minOrMoreNode.Children)
                {
                    output.Or(BuildLookup(builder, (LookupNode)child.Children[0]));
                }

                return output;
            }
        }

        static IPattern BuildSequence(PatternBuilder builder, INode node)
        {
            var output = SequencePattern.Create();
            foreach (var child in node.Children)
            {
                output.Then(BuildLookup(builder, (LookupNode)child));
            }

            return output;
        }

        static IPattern BuildQuantifier(PatternBuilder builder, INode node)
        {
            var pattern = BuildLookup(builder, (LookupNode)node.Children[0]);
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

        static IPattern BuildTerm(PatternBuilder builder, INode node)
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
                        return BuildLookup(builder, (LookupNode)sequenceNode.Children[1]);
                    }
                case 2:
                    {
                        var terminalNode = root.Node.Children[0];
                        return LookupPattern.Create(terminalNode.Value);
                    }
                default: throw new Exception();
            }
        }
    }
}
