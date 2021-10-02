using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class PegParserContext
    {
        public Dictionary<string, IPegPattern> Patterns { get; }

        public PegParserContext()
        {
            Patterns = new Dictionary<string, IPegPattern>();
        }
    }

    public class PegBuilderContext
    {
        public Dictionary<string, Func<PegBuilderContext, INode, IPegPattern>> Builders { get; }

        public PegBuilderContext()
        {
            Builders = new Dictionary<string, Func<PegBuilderContext, INode, IPegPattern>>();
        }

        public IPegPattern BuildKeyPattern(INode node)
        {
            var keyNode = (KeyNode)node;
            return Builders[keyNode.Key](this, keyNode.KeyValue);
        }
    }

    public static class OperatorInfoPegBuilder
    {
        public static PegBuilderContext CreateBuilder()
        {
            var output = new PegBuilderContext();

            output.Builders["sequence"] = BuildSequence;
            output.Builders["choice"] = BuildChoice;
            output.Builders["term"] = BuildTerm;
            output.Builders["quantifier"] = BuildQuantifier;

            return output;
        }

        static IPegPattern BuildChoice(PegBuilderContext ctx, INode node)
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

        static IPegPattern BuildSequence(PegBuilderContext ctx, INode node)
        {
            var output = SequencePattern.Create();
            foreach (var child in node.Children)
            {
                output.Then(ctx.BuildKeyPattern(child));
            }

            return output;
        }

        static IPegPattern BuildQuantifier(PegBuilderContext ctx, INode node)
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

        static IPegPattern BuildTerm(PegBuilderContext ctx, INode node)
        {
            var root = (ChoiceNode)node;

            switch (root.ChoiceIndex)
            {
                case 0:
                    {
                        var choiceNode = (ChoiceNode)root.ChoiceValue;
                        var terminalNode = choiceNode.ChoiceValue;

                        switch (choiceNode.ChoiceIndex)
                        {
                            case 0:
                                {
                                    string? pattern = terminalNode.Value[1..^1].Replace("``", "`");
                                    var lexerPattern = LexerPattern.FromRegex(pattern);
                                    return TerminalPattern.Create(lexerPattern);
                                }
                            case 1:
                                {
                                    string? pattern = terminalNode.Value[1..^1].Replace("''", "'");
                                    var lexerPattern = LexerPattern.FromPlainText(pattern);
                                    return TerminalPattern.Create(lexerPattern);
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
                        var terminalNode = root.ChoiceValue;
                        return KeyPattern.Create(terminalNode.Value);
                    }
                default: throw new Exception();
            }
        }
    }
}
