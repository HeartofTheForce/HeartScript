using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Nodes;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    class PegResult
    {
        public int CharIndex { get; private set; }
        public INode? Value { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static PegResult Success(int charIndex, INode value)
        {
            return new PegResult()
            {
                CharIndex = charIndex,
                Value = value,
            };
        }

        public static PegResult Error(int charIndex, string message)
        {
            return new PegResult()
            {
                CharIndex = charIndex,
                ErrorMessage = message,
            };
        }
    }

    class TerminalPattern : IPegPattern
    {
        private readonly LexerPattern _lexerPattern;

        private TerminalPattern(LexerPattern lexerPattern)
        {
            _lexerPattern = lexerPattern;
        }

        public static TerminalPattern Create(LexerPattern lexerPattern)
        {
            return new TerminalPattern(lexerPattern);
        }

        public PegResult Parse(PegContext ctx, Lexer lexer)
        {
            if (lexer.TryEat(_lexerPattern, out var current))
                return PegResult.Success(current.CharIndex, new PegNode(current.Value));

            return PegResult.Error(lexer.Offset, $"Expected match @ {lexer.Offset}, {_lexerPattern}");
        }
    }

    class SequencePattern : IPegPattern
    {
        private readonly List<IPegPattern> _patterns;

        private SequencePattern()
        {
            _patterns = new List<IPegPattern>();
        }

        public static SequencePattern Create()
        {
            return new SequencePattern();
        }

        public SequencePattern Then(IPegPattern pattern)
        {
            _patterns.Add(pattern);
            return this;
        }

        public PegResult Parse(PegContext ctx, Lexer lexer)
        {
            int startIndex = lexer.Offset;

            var output = new List<INode>();
            foreach (var pattern in _patterns)
            {
                var result = pattern.Parse(ctx, lexer);

                if (result.ErrorMessage != null)
                {
                    lexer.Offset = startIndex;
                    return result;
                }

                if (result.Value != null)
                    output.Add(result.Value);
            }

            return PegResult.Success(startIndex, new PegNode(output));
        }
    }

    class ChoiceNode : PegNode
    {
        public int ChoiceIndex { get; }
        public INode ChoiceValue => Children[0];

        public ChoiceNode(int choiceIndex, INode choiceValue) : base(choiceValue)
        {
            ChoiceIndex = choiceIndex;
        }
    }

    class ChoicePattern : IPegPattern
    {
        private readonly List<IPegPattern> _patterns;

        private ChoicePattern()
        {
            _patterns = new List<IPegPattern>();
        }

        public static ChoicePattern Create()
        {
            return new ChoicePattern();
        }

        public ChoicePattern Or(IPegPattern pattern)
        {
            _patterns.Add(pattern);
            return this;
        }

        public PegResult Parse(PegContext ctx, Lexer lexer)
        {
            int startIndex = lexer.Offset;

            PegResult? furthestResult = null;
            for (int i = 0; i < _patterns.Count; i++)
            {
                var result = _patterns[i].Parse(ctx, lexer);

                if (result.Value != null)
                    return PegResult.Success(result.CharIndex, new ChoiceNode(i, result.Value));

                if (furthestResult == null || result.CharIndex > furthestResult.CharIndex)
                    furthestResult = result;

                if (result.ErrorMessage != null)
                    lexer.Offset = startIndex;
            }

            return furthestResult!;
        }
    }

    class QuantifierPattern : IPegPattern
    {
        private readonly int _min;
        private readonly int? _max;
        private readonly IPegPattern _pattern;

        private QuantifierPattern(int min, int? max, IPegPattern pattern)
        {
            if (max < min)
                throw new ArgumentException($"{nameof(max)} < {nameof(min)}");

            _min = min;
            _max = max;
            _pattern = pattern;
        }

        public static QuantifierPattern MinOrMore(int min, IPegPattern pattern)
        {
            return new QuantifierPattern(min, null, pattern);
        }

        public static QuantifierPattern Optional(IPegPattern pattern)
        {
            return new QuantifierPattern(0, 1, pattern);
        }

        public PegResult Parse(PegContext ctx, Lexer lexer)
        {
            int startIndex = lexer.Offset;

            PegResult? result = null;
            var output = new List<INode>();
            while (_max == null || output.Count < _max)
            {
                result = _pattern.Parse(ctx, lexer);

                if (result.ErrorMessage != null)
                    break;

                if (result.Value != null)
                    output.Add(result.Value);
            }

            if (output.Count >= _min)
                return PegResult.Success(startIndex, new PegNode(output));
            else if (result != null)
                return result;
            else
                throw new Exception();
        }
    }

    class KeyNode : PegNode
    {
        public string Key { get; }
        public INode KeyValue => Children[0];

        public KeyNode(string key, INode keyValue) : base(keyValue)
        {
            Key = key;
        }
    }

    class KeyPattern : IPegPattern
    {
        private readonly string _key;

        private KeyPattern(string key)
        {
            _key = key;
        }

        public static KeyPattern Create(string key)
        {
            return new KeyPattern(key);
        }

        public PegResult Parse(PegContext ctx, Lexer lexer)
        {
            var result = ctx.Patterns[_key].Parse(ctx, lexer);
            if (result.ErrorMessage != null)
                return result;

            if (result.Value != null)
                return PegResult.Success(result.CharIndex, new KeyNode(_key, result.Value));

            throw new Exception();
        }
    }

    interface IPegPattern
    {
        PegResult Parse(PegContext ctx, Lexer lexer);
    }

    class PegContext
    {
        public Dictionary<string, IPegPattern> Patterns { get; }

        public PegContext()
        {
            Patterns = new Dictionary<string, IPegPattern>();
        }
    }

    static class Peg
    {
        private static readonly LexerPattern s_regex = LexerPattern.FromRegex("`(?:``|[^`])*`");
        private static readonly LexerPattern s_plainText = LexerPattern.FromRegex("'(?:''|[^'])*'");

        static IPegPattern BuildParser()
        {
            string input = File.ReadAllText("src/peg.ops");
            var lexer = new Lexer(input);

            var ctx = new PegContext();
            ctx.Patterns["term"] = ChoicePattern.Create()
                    .Or(ChoicePattern.Create()
                        .Or(TerminalPattern.Create(s_regex))
                        .Or(TerminalPattern.Create(s_plainText)))
                    .Or(SequencePattern.Create()
                        .Then(TerminalPattern.Create(LexerPattern.FromPlainText("(")))
                        .Then(KeyPattern.Create("choice"))
                        .Then(TerminalPattern.Create(LexerPattern.FromPlainText(")"))))
                    .Or(TerminalPattern.Create(LexerPattern.FromRegex("\\w+")));

            ctx.Patterns["sequence"] = QuantifierPattern.MinOrMore(
                1,
                KeyPattern.Create("quantifier")
            );

            ctx.Patterns["quantifier"] = SequencePattern.Create()
                .Then(KeyPattern.Create("term"))
                .Then(QuantifierPattern.Optional(
                    ChoicePattern.Create()
                        .Or(TerminalPattern.Create(LexerPattern.FromPlainText("?")))
                        .Or(TerminalPattern.Create(LexerPattern.FromPlainText("*")))
                        .Or(TerminalPattern.Create(LexerPattern.FromPlainText("+")))));

            ctx.Patterns["choice"] = SequencePattern.Create()
                .Then(KeyPattern.Create("sequence"))
                .Then(QuantifierPattern.MinOrMore(
                        0,
                        SequencePattern.Create()
                            .Then(TerminalPattern.Create(LexerPattern.FromPlainText("/")))
                            .Then(KeyPattern.Create("sequence"))));

            var builderPattern = KeyPattern.Create("choice");

            var result = builderPattern.Parse(ctx, lexer);
            var parserPattern = BuildRoot(result);

            return parserPattern;
        }

        static IPegPattern BuildRoot(PegResult result)
        {
            return BuildKey(result.Value);
        }

        static IPegPattern BuildKey(INode node)
        {
            var keyNode = (KeyNode)node;
            return keyNode.Key switch
            {
                "sequence" => BuildSequence(keyNode.KeyValue),
                "choice" => BuildChoice(keyNode.KeyValue),
                "term" => BuildTerm(keyNode.KeyValue),
                "quantifier" => BuildQuantifier(keyNode.KeyValue),
                _ => throw new Exception($"Unexpected Key, {keyNode.Key}"),
            };
        }

        static IPegPattern BuildChoice(INode node)
        {
            var minOrMoreNode = node.Children[1];

            if (minOrMoreNode.Children.Count == 0)
                return BuildKey(node.Children[0]);
            else
            {
                var output = ChoicePattern.Create()
                   .Or(BuildKey(node.Children[0]));

                foreach (var child in minOrMoreNode.Children)
                {
                    output.Or(BuildKey(child.Children[0]));
                }

                return output;
            }
        }

        static IPegPattern BuildSequence(INode node)
        {
            var output = SequencePattern.Create();
            foreach (var child in node.Children)
            {
                output.Then(BuildKey(child));
            }

            return output;
        }

        static IPegPattern BuildQuantifier(INode node)
        {
            var pattern = BuildKey(node.Children[0]);
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

        static IPegPattern BuildTerm(INode node)
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
                        return BuildKey(sequenceNode.Children[1]);
                    }
                case 2:
                    {
                        var terminalNode = root.ChoiceValue;
                        return KeyPattern.Create(terminalNode.Value);
                    }
                default: throw new Exception();
            }
        }

        public static void Test(Lexer lexer)
        {
            var ctx = new PegContext();
            ctx.Patterns["expr"] = TerminalPattern.Create(LexerPattern.FromRegex("\\w+"));

            var pattern = BuildParser();
            var result = pattern.Parse(ctx, lexer);
        }
    }
}
