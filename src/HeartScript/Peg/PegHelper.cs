using System;
using System.Collections.Generic;
using HeartScript.Parsing;
using HeartScript.Peg.Nodes;
using HeartScript.Peg.Patterns;
#pragma warning disable IDE0066

namespace HeartScript.Peg
{
    public static class PegHelper
    {
        private static readonly Dictionary<string, Func<INode, IPattern>> s_builders = new Dictionary<string, Func<INode, IPattern>>()
        {
            ["peg"] = BuildPeg,
            ["choice"] = BuildChoice,
            ["sequence"] = BuildSequence,
            ["quantifier"] = BuildQuantifier,
            ["term"] = BuildTerm,
        };

        private static readonly LexerPattern s_regex = LexerPattern.FromRegex("`(?:``|[^`])*`");
        private static readonly LexerPattern s_plainText = LexerPattern.FromRegex("'(?:''|[^'])*'");
        public static readonly LexerPattern NonSignificant = LexerPattern.FromRegex("\\s*");

        public static IPattern TrimLeft(this IPattern pattern)
        {
            return SequencePattern.Create()
                .Discard(NonSignificant)
                .Then(pattern);
        }

        public static IPattern TrimRight(this IPattern pattern)
        {
            return SequencePattern.Create()
                .Then(pattern)
                .Discard(NonSignificant);
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
                            .Discard(LexerPattern.FromPlainText("/").TrimRight())
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
                        .Discard(LexerPattern.FromPlainText("(").TrimRight())
                        .Then(LookupPattern.Create("choice"))
                        .Discard(LexerPattern.FromPlainText(")").TrimRight()))
                    .Or(LexerPattern.FromRegex("\\w+").TrimRight());

            return parser;
        }

        public static IPattern BuildLookup(INode node)
        {
            var lookupNode = (LookupNode)node;

            if (lookupNode.Name == null)
                throw new ArgumentException($"{nameof(node.Name)} cannot be null");

            return s_builders[lookupNode.Name](lookupNode.Node);
        }

        static IPattern BuildPeg(INode node)
        {
            return BuildLookup(node);
        }

        static IPattern BuildChoice(INode node)
        {
            var leftNode = BuildLookup(node.Children[0]);
            var minOrMoreNode = node.Children[1];

            if (minOrMoreNode.Children.Count == 0)
                return leftNode;

            var output = ChoicePattern.Create()
               .Or(leftNode);

            foreach (var child in minOrMoreNode.Children)
            {
                output.Or(BuildLookup(child));
            }

            return output;
        }

        static IPattern BuildSequence(INode node)
        {
            if (node.Children.Count == 1)
                return BuildLookup(node.Children[0]);

            var output = SequencePattern.Create();
            foreach (var child in node.Children)
            {
                output.Then(BuildLookup(child));
            }

            return output;
        }

        static IPattern BuildQuantifier(INode node)
        {
            var pattern = BuildLookup(node.Children[0]);
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

        static IPattern BuildTerm(INode node)
        {
            var root = (ChoiceNode)node;

            switch (root.ChoiceIndex)
            {
                case 0:
                    {
                        var choiceNode = (ChoiceNode)root.Node;
                        var terminalNode = choiceNode.Node;

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
                    return BuildLookup(root.Node);
                case 2:
                    return LookupPattern.Create(root.Node.Value);
                default:
                    throw new Exception();
            }
        }
    }
}
