using System;
using System.Collections.Generic;
using HeartScript.Parsing;
using HeartScript.Peg.Patterns;
#pragma warning disable IDE0066

namespace HeartScript.Peg
{
    public static class PegHelper
    {
        private static readonly Dictionary<string, Func<IParseNode, IPattern>> s_builders = new Dictionary<string, Func<IParseNode, IPattern>>()
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

        public static PatternParser CreatePegParser()
        {
            var parser = new PatternParser();

            parser.Patterns["peg"] = SequencePattern.Create()
                .Discard(NonSignificant)
                .Then(QuantifierPattern.MinOrMore(1, LookupPattern.Create("rule")));

            parser.Patterns["expr_rule"] = SequencePattern.Create()
                .Discard(NonSignificant)
                .Then(LexerPattern.FromPlainText("["))
                .Then(QuantifierPattern.MinOrMore(
                    0,
                    SequencePattern.Create()
                        .Discard(NonSignificant)
                        .Discard(LexerPattern.FromPlainText("("))
                        .Discard(NonSignificant)
                        .Then(ChoicePattern.Create()
                            .Or(LexerPattern.FromRegex("\\d"))
                            .Or(LexerPattern.FromPlainText("none")))
                        .Discard(NonSignificant)
                        .Then(ChoicePattern.Create()
                            .Or(LexerPattern.FromRegex("\\d"))
                            .Or(LexerPattern.FromPlainText("none")))
                        .Then(LookupPattern.Create("choice"))
                        .Discard(NonSignificant)
                        .Discard(LexerPattern.FromPlainText(")"))
                        .Discard(NonSignificant)
                        .Discard(LexerPattern.FromPlainText(","))));

            parser.Patterns["rule"] = SequencePattern.Create()
                .Then(LookupPattern.Create("rule_head"))
                .Then(LookupPattern.Create("choice"));

            parser.Patterns["rule_head"] = SequencePattern.Create()
                .Discard(NonSignificant)
                .Then(LexerPattern.FromRegex("\\w+"))
                .Discard(NonSignificant)
                .Then(LexerPattern.FromPlainText("->"));

            parser.Patterns["choice"] = SequencePattern.Create()
                .Then(LookupPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                    0,
                    SequencePattern.Create()
                        .Discard(NonSignificant)
                        .Discard(LexerPattern.FromPlainText("/"))
                        .Discard(LookaheadPattern.Negative(LookupPattern.Create("rule_head")))
                        .Then(LookupPattern.Create("sequence"))));

            parser.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
                LookupPattern.Create("quantifier"));

            parser.Patterns["quantifier"] = SequencePattern.Create()
                .Then(LookupPattern.Create("term"))
                .Discard(NonSignificant)
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(LexerPattern.FromPlainText("?"))
                        .Or(LexerPattern.FromPlainText("*"))
                        .Or(LexerPattern.FromPlainText("+"))));

            parser.Patterns["term"] = SequencePattern.Create()
                .Discard(NonSignificant)
                .Then(ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(s_regex)
                        .Or(s_plainText))
                    .Or(SequencePattern.Create()
                        .Discard(LexerPattern.FromPlainText("("))
                        .Discard(NonSignificant)
                        .Then(LookupPattern.Create("choice"))
                        .Discard(NonSignificant)
                        .Discard(LexerPattern.FromPlainText(")")))
                    .Or(LexerPattern.FromRegex("\\w+")));

            return parser;
        }

        public static IPattern BuildLookup(IParseNode node)
        {
            var lookupNode = (LookupNode)node;

            if (lookupNode.Key == null)
                throw new ArgumentException($"{nameof(LookupNode.Key)} cannot be null");

            return s_builders[lookupNode.Key](lookupNode.Node);
        }

        static IPattern BuildPeg(IParseNode node)
        {
            return BuildLookup(node);
        }

        static IPattern BuildChoice(IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var leftNode = BuildLookup(sequenceNode.Children[0]);
            var minOrMoreNode = (QuantifierNode)sequenceNode.Children[1];

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

        static IPattern BuildSequence(IParseNode node)
        {
            var quantifierNode = (QuantifierNode)node;
            if (quantifierNode.Children.Count == 1)
                return BuildLookup(quantifierNode.Children[0]);

            var output = SequencePattern.Create();
            foreach (var child in quantifierNode.Children)
            {
                output.Then(BuildLookup(child));
            }

            return output;
        }

        static IPattern BuildQuantifier(IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var pattern = BuildLookup(sequenceNode.Children[0]);
            var optional = (QuantifierNode)sequenceNode.Children[1];

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

        static IPattern BuildTerm(IParseNode node)
        {
            var root = (ChoiceNode)node;

            switch (root.ChoiceIndex)
            {
                case 0:
                    {
                        var choiceNode = (ChoiceNode)root.Node;
                        var valueNode = (ValueNode)choiceNode.Node;

                        switch (choiceNode.ChoiceIndex)
                        {
                            case 0:
                                {
                                    string? pattern = valueNode.Value[1..^1].Replace("``", "`");
                                    return SequencePattern.Create()
                                        .Then(LexerPattern.FromRegex(pattern))
                                        .Discard(NonSignificant);
                                }
                            case 1:
                                {
                                    string? pattern = valueNode.Value[1..^1].Replace("''", "'");
                                    return SequencePattern.Create()
                                        .Then(LexerPattern.FromPlainText(pattern))
                                        .Discard(NonSignificant);
                                }
                            default: throw new Exception();
                        }
                    };
                case 1:
                    return BuildLookup(root.Node);
                case 2:
                    {
                        var valueNode = (ValueNode)root.Node;
                        return LookupPattern.Create(valueNode.Value);
                    }
                default:
                    throw new Exception();
            }
        }
    }
}
