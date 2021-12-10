using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Expressions;
using HeartScript.Parsing;
using HeartScript.Peg.Patterns;

namespace HeartScript.Peg
{
    public static class PegHelper
    {
        private static readonly LexerPattern s_regex = LexerPattern.FromRegex("`(?:``|[^`])*`");
        private static readonly LexerPattern s_plainText = LexerPattern.FromRegex("'(?:''|[^'])*'");
        private static readonly LexerPattern s_identifier = LexerPattern.FromRegex("[_a-zA-Z]\\w*");
        private static readonly IPattern s_nonSignificant = QuantifierPattern.MinOrMore(0,
            ChoicePattern.Create()
                .Or(LexerPattern.FromRegex("#.*"))
                .Or(LexerPattern.FromRegex("\\s+")));

        public static IPattern TrimRight(this LexerPattern pattern)
        {
            return SequencePattern.Create()
                .Then(pattern)
                .Discard(s_nonSignificant);
        }

        public static PatternParser BuildPatternParser(string path)
        {
            string input = File.ReadAllText(path);

            var pegParser = CreatePegParser();
            var ctx = new ParserContext(input);

            var root = QuantifierPattern.MinOrMore(
                1,
                SequencePattern.Create()
                    .Discard(s_nonSignificant)
                    .Then(LookupPattern.Create("rule")));

            var result = root.Match(pegParser, ctx);

            ctx.AssertComplete();
            if (result == null)
                throw new ArgumentException(nameof(ctx.Exception));

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
                .Then(ChoicePattern.Create()
                    .Or(LookupPattern.Create("expr"))
                    .Or(LookupPattern.Create("choice")));

            parser.Patterns["rule_head"] = SequencePattern.Create()
                .Then(s_identifier.TrimRight())
                .Discard(LexerPattern.FromPlainText("->").TrimRight());

            parser.Patterns["choice"] = SequencePattern.Create()
                .Then(LookupPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                    0,
                    SequencePattern.Create()
                        .Discard(LexerPattern.FromPlainText("/").TrimRight())
                        .Then(LookupPattern.Create("sequence"))));

            parser.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
               LookupPattern.Create("label"));

            parser.Patterns["label"] = SequencePattern.Create()
                .Then(QuantifierPattern.Optional(
                        SequencePattern.Create()
                            .Then(s_plainText.TrimRight())
                            .Discard(LexerPattern.FromPlainText(":").TrimRight())))
                .Then(LookupPattern.Create("quantifier"));

            parser.Patterns["quantifier"] = SequencePattern.Create()
                .Then(LookupPattern.Create("term"))
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(LexerPattern.FromPlainText("?").TrimRight())
                        .Or(LexerPattern.FromPlainText("*").TrimRight())
                        .Or(LexerPattern.FromPlainText("+").TrimRight())));

            parser.Patterns["term"] = SequencePattern.Create()
                .Discard(PredicatePattern.Negative(LookupPattern.Create("rule_head")))
                .Discard(PredicatePattern.Negative(LookupPattern.Create("expr_head")))
                .Then(ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(s_regex.TrimRight())
                        .Or(s_plainText.TrimRight()))
                    .Or(SequencePattern.Create()
                        .Discard(LexerPattern.FromPlainText("(").TrimRight())
                        .Then(LookupPattern.Create("choice"))
                        .Discard(LexerPattern.FromPlainText(")").TrimRight()))
                    .Or(s_identifier.TrimRight()));

            var digits = LexerPattern.FromRegex("\\d+");
            var none = LexerPattern.FromPlainText("none");
            parser.Patterns["expr_head"] = SequencePattern.Create()
                .Then(s_plainText.TrimRight())
                .Then(ChoicePattern.Create()
                    .Or(digits.TrimRight())
                    .Or(none.TrimRight()))
                .Then(ChoicePattern.Create()
                    .Or(digits.TrimRight())
                    .Or(none.TrimRight()));

            parser.Patterns["expr"] = SequencePattern.Create()
                .Discard(LexerPattern.FromPlainText("[").TrimRight())
                .Then(QuantifierPattern.MinOrMore(
                    0,
                    SequencePattern.Create()
                        .Then(LookupPattern.Create("expr_head"))
                        .Then(LookupPattern.Create("choice"))))
                .Discard(LexerPattern.FromPlainText("]").TrimRight());

            return parser;
        }

        static IPattern BuildChoice(IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var leftPattern = BuildSequence(sequenceNode.Children[0]);

            var minOrMoreNode = (QuantifierNode)sequenceNode.Children[1];
            if (minOrMoreNode.Children.Count == 0)
                return leftPattern;

            var output = ChoicePattern.Create()
               .Or(leftPattern);

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
                return BuildLabel(quantifierNode.Children[0]);

            var output = SequencePattern.Create();
            foreach (var child in quantifierNode.Children)
            {
                output.Then(BuildLabel(child));
            }

            return output;
        }

        static IPattern BuildLabel(IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var optional = (QuantifierNode)sequenceNode.Children[0];
            var pattern = BuildQuantifier(sequenceNode.Children[1]);

            if (optional.Children.Count == 0)
                return pattern;

            var valueNode = (ValueNode)optional.Children[0];
            string label = valueNode.Value[1..^1].Replace("''", "'");

            return LabelPattern.Create(label, pattern);
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
                                    string pattern = valueNode.Value[1..^1].Replace("``", "`");
                                    return LexerPattern.FromRegex(pattern).TrimRight();
                                }
                            case 1:
                                {
                                    string pattern = valueNode.Value[1..^1].Replace("''", "'");
                                    return LexerPattern.FromPlainText(pattern).TrimRight();
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
