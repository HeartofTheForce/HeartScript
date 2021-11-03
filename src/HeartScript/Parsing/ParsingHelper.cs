using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Expressions;
using HeartScript.Peg.Patterns;
#pragma warning disable IDE0066

namespace HeartScript.Parsing
{
    public static class ParsingHelper
    {
        private static readonly Dictionary<string, Func<IParseNode, IPattern>> s_builders = new Dictionary<string, Func<IParseNode, IPattern>>()
        {
            ["choice"] = BuildChoice,
            ["sequence"] = BuildSequence,
            ["quantifier"] = BuildQuantifier,
            ["term"] = BuildTerm,
            ["expr"] = BuildExpr,
        };

        private static readonly IPattern s_regex = LexerPattern.FromRegex("`(?:``|[^`])*`");
        private static readonly IPattern s_plainText = LexerPattern.FromRegex("'(?:''|[^'])*'");
        private static readonly IPattern s_identifier = LexerPattern.FromRegex("[_a-zA-Z]\\w*");
        private static readonly IPattern s_nonSignificant = QuantifierPattern.MinOrMore(0,
            ChoicePattern.Create()
                .Or(LexerPattern.FromRegex("#[^(\\r\\n|\\r|\\n)]*"))
                .Or(LexerPattern.FromRegex("\\s+")));

        public static PatternParser BuildPatternParser(string path)
        {
            string input = File.ReadAllText(path);

            var pegParser = CreatePegParser();
            var ctx = new ParserContext(input);

            var root = QuantifierPattern.MinOrMore(1, SequencePattern.Create()
                .Discard(s_nonSignificant)
                .Then(LookupPattern.Create("rule")));

            var result = root.Match(pegParser, ctx);

            if (result == null)
            {
                if (ctx.Exception != null)
                    throw ctx.Exception;
                else
                    throw new ArgumentException(nameof(ctx.Exception));
            }

            var output = new PatternParser();
            var rules = (QuantifierNode)result;
            foreach (var child in rules.Children)
            {
                var sequenceNode = (SequenceNode)child;

                var ruleNameNode = (ValueNode)sequenceNode.Children[0];
                string ruleName = ruleNameNode.Value;

                var choiceNode = (ChoiceNode)sequenceNode.Children[1];
                IPattern rulePattern;
                switch (choiceNode.ChoiceIndex)
                {
                    case 0: rulePattern = BuildExpr(choiceNode.Node); break;
                    case 1: rulePattern = BuildChoice(choiceNode.Node); break;
                    default: throw new NotImplementedException();
                }

                output.Patterns[ruleName] = rulePattern;
            }

            return output;
        }

        private static PatternParser CreatePegParser()
        {
            var parser = new PatternParser();

            parser.Patterns["rule"] = SequencePattern.Create()
                .Then(LookupPattern.Create("rule_head"))
                .Discard(s_nonSignificant)
                .Then(ChoicePattern.Create()
                    .Or(LookupPattern.Create("expr"))
                    .Or(LookupPattern.Create("choice")));

            parser.Patterns["rule_head"] = SequencePattern.Create()
                .Then(s_identifier)
                .Discard(s_nonSignificant)
                .Discard(LexerPattern.FromPlainText("->"));

            parser.Patterns["choice"] = SequencePattern.Create()
                .Then(LookupPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                    0,
                    SequencePattern.Create()
                        .Discard(s_nonSignificant)
                        .Discard(LexerPattern.FromPlainText("/"))
                        .Then(LookupPattern.Create("sequence"))));

            parser.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
                LookupPattern.Create("quantifier"));

            parser.Patterns["quantifier"] = SequencePattern.Create()
                .Then(LookupPattern.Create("term"))
                .Discard(s_nonSignificant)
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(LexerPattern.FromPlainText("?"))
                        .Or(LexerPattern.FromPlainText("*"))
                        .Or(LexerPattern.FromPlainText("+"))));

            parser.Patterns["term"] = SequencePattern.Create()
                .Discard(s_nonSignificant)
                .Discard(LookaheadPattern.Negative(LookupPattern.Create("rule_head")))
                .Discard(LookaheadPattern.Negative(LookupPattern.Create("expr_head")))
                .Then(ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(s_regex)
                        .Or(s_plainText))
                    .Or(SequencePattern.Create()
                        .Discard(LexerPattern.FromPlainText("("))
                        .Discard(s_nonSignificant)
                        .Then(LookupPattern.Create("choice"))
                        .Discard(s_nonSignificant)
                        .Discard(LexerPattern.FromPlainText(")")))
                    .Or(s_identifier));

            parser.Patterns["expr_head"] = SequencePattern.Create()
                .Then(s_plainText)
                .Discard(s_nonSignificant)
                .Then(ChoicePattern.Create()
                    .Or(LexerPattern.FromRegex("\\d+"))
                    .Or(LexerPattern.FromPlainText("none")))
                .Discard(s_nonSignificant)
                .Then(ChoicePattern.Create()
                    .Or(LexerPattern.FromRegex("\\d+"))
                    .Or(LexerPattern.FromPlainText("none")));

            parser.Patterns["expr"] = SequencePattern.Create()
                .Discard(s_nonSignificant)
                .Discard(LexerPattern.FromPlainText("["))
                .Then(QuantifierPattern.MinOrMore(
                    0,
                    SequencePattern.Create()
                        .Discard(s_nonSignificant)
                        .Then(LookupPattern.Create("expr_head"))
                        .Discard(s_nonSignificant)
                        .Then(LookupPattern.Create("choice"))))
                .Discard(s_nonSignificant)
                .Discard(LexerPattern.FromPlainText("]"));

            return parser;
        }

        static IPattern BuildChoice(IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var leftNode = BuildSequence(sequenceNode.Children[0]);
            var minOrMoreNode = (QuantifierNode)sequenceNode.Children[1];

            if (minOrMoreNode.Children.Count == 0)
                return leftNode;

            var output = ChoicePattern.Create()
               .Or(leftNode);

            foreach (var child in minOrMoreNode.Children)
            {
                output.Or(BuildSequence(child));
            }

            return output;
        }

        static IPattern BuildSequence(IParseNode node)
        {
            var quantifierNode = (QuantifierNode)node;
            if (quantifierNode.Children.Count == 1)
                return BuildQuantifier(quantifierNode.Children[0]);

            var output = SequencePattern.Create();
            foreach (var child in quantifierNode.Children)
            {
                output.Then(BuildQuantifier(child));
            }

            return output;
        }

        static IPattern BuildQuantifier(IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var pattern = BuildTerm(sequenceNode.Children[0]);
            var optional = (QuantifierNode)sequenceNode.Children[1];

            if (optional.Children.Count == 0)
                return pattern;

            var choice = (ChoiceNode)optional.Children[0];
            switch (choice.ChoiceIndex)
            {
                case 0: return QuantifierPattern.Optional(pattern);
                case 1: return QuantifierPattern.MinOrMore(0, pattern);
                case 2: return QuantifierPattern.MinOrMore(1, pattern);
                default: throw new NotImplementedException();
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
                                        .Discard(s_nonSignificant);
                                }
                            case 1:
                                {
                                    string? pattern = valueNode.Value[1..^1].Replace("''", "'");
                                    return SequencePattern.Create()
                                        .Then(LexerPattern.FromPlainText(pattern))
                                        .Discard(s_nonSignificant);
                                }
                            default: throw new NotImplementedException();
                        }
                    };
                case 1:
                    return BuildChoice(root.Node);
                case 2:
                    {
                        var valueNode = (ValueNode)root.Node;
                        return LookupPattern.Create(valueNode.Value);
                    }
                default: throw new NotImplementedException();
            }
        }

        static IPattern BuildExpr(IParseNode node)
        {
            var quantifierNode = (QuantifierNode)node;

            var operatorInfos = new List<OperatorInfo>();
            foreach (var child in quantifierNode.Children)
            {
                var sequenceNode = (SequenceNode)child;

                var headNode = (SequenceNode)sequenceNode.Children[0];
                var keyNode = (ValueNode)headNode.Children[0];
                string key = keyNode.Value[1..^1];

                var leftNode = (ChoiceNode)headNode.Children[1];
                uint? leftPrecedence = null;
                if (leftNode.ChoiceIndex == 0)
                {
                    var valueNode = (ValueNode)leftNode.Node;
                    leftPrecedence = uint.Parse(valueNode.Value);
                }

                var rightNode = (ChoiceNode)headNode.Children[2];
                uint? rightPrecedence = null;
                if (rightNode.ChoiceIndex == 0)
                {
                    var valueNode = (ValueNode)rightNode.Node;
                    rightPrecedence = uint.Parse(valueNode.Value);
                }

                var patternNode = sequenceNode.Children[1];
                var pattern = BuildChoice(patternNode);

                var operatorInfo = new OperatorInfo(key, pattern, leftPrecedence, rightPrecedence);
                operatorInfos.Add(operatorInfo);
            }

            return new ExpressionPattern(operatorInfos);
        }
    }
}
