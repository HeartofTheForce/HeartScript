using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Cli
{
    interface IPegNode
    {
    }

    class PegResult
    {
        public int CharIndex { get; private set; }
        public IPegNode? Value { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static PegResult Success(int charIndex, IPegNode value)
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

    class TerminalNode : IPegNode
    {
        public string Value { get; }

        public TerminalNode(string value)
        {
            Value = value;
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

        public PegResult Parse(PegContext ctx)
        {
            if (ctx.Lexer.TryEat(_lexerPattern, out var current))
            {
                return PegResult.Success(current.CharIndex, new TerminalNode(current.Value));
            }

            return PegResult.Error(ctx.Lexer.Offset, $"Expected match @ {ctx.Lexer.Offset}, {_lexerPattern}");
        }
    }

    class SequenceNode : IPegNode
    {
        public IPegNode[] Values { get; }

        public SequenceNode(IEnumerable<IPegNode> values)
        {
            Values = values.ToArray();
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

        public PegResult Parse(PegContext ctx)
        {
            int startIndex = ctx.Lexer.Offset;

            var output = new List<IPegNode>();
            foreach (var pattern in _patterns)
            {
                var result = pattern.Parse(ctx);

                if (result.ErrorMessage != null)
                {
                    ctx.Lexer.Offset = startIndex;
                    return result;
                }

                if (result.Value != null)
                    output.Add(result.Value);
            }

            return PegResult.Success(startIndex, new SequenceNode(output));
        }
    }

    class ChoiceNode : IPegNode
    {
        public int ChoiceIndex { get; }
        public IPegNode Value { get; }

        public ChoiceNode(int choiceIndex, IPegNode value)
        {
            ChoiceIndex = choiceIndex;
            Value = value;
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

        public PegResult Parse(PegContext ctx)
        {
            int startIndex = ctx.Lexer.Offset;

            PegResult? furthestResult = null;
            for (int i = 0; i < _patterns.Count; i++)
            {
                var result = _patterns[i].Parse(ctx);

                if (result.Value != null)
                    return PegResult.Success(result.CharIndex, new ChoiceNode(i, result.Value));

                if (furthestResult == null || result.CharIndex > furthestResult.CharIndex)
                    furthestResult = result;

                if (result.ErrorMessage != null)
                    ctx.Lexer.Offset = startIndex;
            }

            return furthestResult!;
        }
    }

    class QuantifierNode : IPegNode
    {
        public IPegNode[] Values { get; }

        public QuantifierNode(IEnumerable<IPegNode> values)
        {
            Values = values.ToArray();
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

        public PegResult Parse(PegContext ctx)
        {
            int startIndex = ctx.Lexer.Offset;

            PegResult? result = null;
            var output = new List<IPegNode>();
            while (_max == null || output.Count < _max)
            {
                result = _pattern.Parse(ctx);

                if (result.ErrorMessage != null)
                    break;

                if (result.Value != null)
                    output.Add(result.Value);
            }

            if (output.Count >= _min)
                return PegResult.Success(startIndex, new QuantifierNode(output));
            else if (result != null)
                return result;
            else
                throw new Exception();
        }
    }

    class KeyNode : IPegNode
    {
        public string Key { get; }
        public IPegNode Value { get; }

        public KeyNode(string key, IPegNode value)
        {
            Key = key;
            Value = value;
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

        public PegResult Parse(PegContext ctx)
        {
            var result = ctx.Patterns[_key].Parse(ctx);
            if (result.ErrorMessage != null)
                return result;

            if (result.Value != null)
                return PegResult.Success(result.CharIndex, new KeyNode(_key, result.Value));

            throw new Exception();
        }
    }

    interface IPegPattern
    {
        PegResult Parse(PegContext ctx);
    }

    class PegContext
    {
        public Lexer Lexer { get; }
        public Dictionary<string, IPegPattern> Patterns { get; }

        public PegContext(Lexer lexer)
        {
            Lexer = lexer;
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
            var ctx = new PegContext(lexer);

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

            var result = builderPattern.Parse(ctx);
            var parserPattern = BuildRoot(result);

            return parserPattern;
        }

        static IPegPattern BuildRoot(PegResult result)
        {
            return BuildKey(result.Value);
        }

        static IPegPattern BuildKey(IPegNode node)
        {
            var keyNode = (KeyNode)node;
            return keyNode.Key switch
            {
                "sequence" => BuildSequence(keyNode.Value),
                "choice" => BuildChoice(keyNode.Value),
                "term" => BuildTerm(keyNode.Value),
                "quantifier" => BuildQuantifier(keyNode.Value),
                _ => throw new Exception($"Unexpected Key, {keyNode.Key}"),
            };
        }

        static IPegPattern BuildChoice(IPegNode node)
        {
            var root = (SequenceNode)node;
            var minOrMoreNode = (QuantifierNode)root.Values[1];

            if (minOrMoreNode.Values.Length == 0)
                return BuildKey(root.Values[0]);
            else
            {
                var output = ChoicePattern.Create()
                   .Or(BuildKey(root.Values[0]));

                foreach (var value in minOrMoreNode.Values)
                {
                    var sequenceNode = (SequenceNode)value;
                    output.Or(BuildKey(sequenceNode.Values[0]));
                }

                return output;
            }
        }

        static IPegPattern BuildSequence(IPegNode node)
        {
            var root = (QuantifierNode)node;

            var output = SequencePattern.Create();
            foreach (var value in root.Values)
            {
                output.Then(BuildKey(value));
            }

            return output;
        }

        static IPegPattern BuildQuantifier(IPegNode node)
        {
            var root = (SequenceNode)node;

            var pattern = BuildKey(root.Values[0]);
            var optional = (QuantifierNode)root.Values[1];

            if (optional.Values.Length == 0)
                return pattern;

            var choice = (ChoiceNode)optional.Values[0];
            return choice.ChoiceIndex switch
            {
                0 => QuantifierPattern.Optional(pattern),
                1 => QuantifierPattern.MinOrMore(0, pattern),
                2 => QuantifierPattern.MinOrMore(1, pattern),
                _ => throw new Exception(),
            };
        }

        static IPegPattern BuildTerm(IPegNode node)
        {
            var root = (ChoiceNode)node;

            switch (root.ChoiceIndex)
            {
                case 0:
                    {
                        var choiceNode = (ChoiceNode)root.Value;
                        var terminalNode = (TerminalNode)choiceNode.Value;

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
                        var sequenceNode = (SequenceNode)root.Value;
                        return BuildKey(sequenceNode.Values[1]);
                    }
                case 2:
                    {
                        var terminalNode = (TerminalNode)root.Value;
                        return KeyPattern.Create(terminalNode.Value);
                    }
                default: throw new Exception();
            }
        }

        public static void Test(Lexer lexer)
        {
            var ctx = new PegContext(lexer);
            ctx.Patterns["expr"] = TerminalPattern.Create(LexerPattern.FromRegex("\\w+"));

            var pattern = BuildParser();
            var result = pattern.Parse(ctx);
        }
    }
}
