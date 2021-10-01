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

    class MinOrMoreNode : IPegNode
    {
        public IPegNode[] Values { get; }

        public MinOrMoreNode(IEnumerable<IPegNode> values)
        {
            Values = values.ToArray();
        }
    }

    class MinOrMorePattern : IPegPattern
    {
        private readonly int _min;
        private readonly IPegPattern _pattern;

        private MinOrMorePattern(int min, IPegPattern pattern)
        {
            _min = min;
            _pattern = pattern;
        }

        public static MinOrMorePattern Create(int min, IPegPattern pattern)
        {
            return new MinOrMorePattern(min, pattern);
        }

        public PegResult Parse(PegContext ctx)
        {
            int startIndex = ctx.Lexer.Offset;
            var output = new List<IPegNode>();

            PegResult result;
            do
            {
                result = _pattern.Parse(ctx);
                if (result.Value != null)
                    output.Add(result.Value);
            } while (result.ErrorMessage == null);

            if (output.Count >= _min)
                return PegResult.Success(startIndex, new MinOrMoreNode(output));
            else
                return result;
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

            ctx.Patterns["sequence"] = MinOrMorePattern.Create(
                1,
                ChoicePattern.Create()
                    .Or(KeyPattern.Create("zero-or-more"))
                    .Or(KeyPattern.Create("term"))
            );

            ctx.Patterns["zero-or-more"] = SequencePattern.Create()
                .Then(KeyPattern.Create("term"))
                .Then(TerminalPattern.Create(LexerPattern.FromPlainText("*")));

            ctx.Patterns["choice"] = SequencePattern.Create()
                .Then(KeyPattern.Create("sequence"))
                .Then(MinOrMorePattern.Create(
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
                "zero-or-more" => BuildZeroOrMore(keyNode.Value),
                _ => throw new Exception($"Unexpected Key, {keyNode.Key}"),
            };
        }

        static IPegPattern BuildChoice(IPegNode node)
        {
            var root = (SequenceNode)node;
            var minOrMoreNode = (MinOrMoreNode)root.Values[1];

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
            var root = (MinOrMoreNode)node;

            var output = SequencePattern.Create();
            foreach (var value in root.Values)
            {
                var choice = (ChoiceNode)value;
                output.Then(BuildKey(choice.Value));
            }

            return output;
        }

        static IPegPattern BuildZeroOrMore(IPegNode node)
        {
            var root = (SequenceNode)node;

            var pattern = BuildKey(root.Values[0]);
            return MinOrMorePattern.Create(0, pattern);
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
