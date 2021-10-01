using System;
using System.Collections.Generic;
using System.IO;
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
        public IEnumerable<IPegNode> Values { get; }

        public SequenceNode(IEnumerable<IPegNode> values)
        {
            Values = values;
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
        public IEnumerable<IPegNode> Values { get; }

        public MinOrMoreNode(IEnumerable<IPegNode> values)
        {
            Values = values;
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

            ctx.Patterns["key"] = TerminalPattern.Create(LexerPattern.FromRegex("\\w+"));
            ctx.Patterns["string"] = ChoicePattern.Create()
                    .Or(TerminalPattern.Create(s_regex))
                    .Or(TerminalPattern.Create(s_plainText));
            ctx.Patterns["term"] = ChoicePattern.Create()
                    .Or(KeyPattern.Create("string"))
                    .Or(SequencePattern.Create()
                        .Then(TerminalPattern.Create(LexerPattern.FromPlainText("(")))
                        .Then(KeyPattern.Create("string"))
                        .Then(TerminalPattern.Create(LexerPattern.FromPlainText(")"))))
                    .Or(KeyPattern.Create("key"))
            ;

            var op = MinOrMorePattern.Create(
                1,
                KeyPattern.Create("term")
            );

            var result = op.Parse(ctx);
            return op;
        }

        public static void Test(Lexer lexer)
        {
            var ctx = new PegContext(lexer);
            var pattern = SequencePattern.Create()
                .Then(TerminalPattern.Create(LexerPattern.FromRegex("\\w+")))
                .Then(TerminalPattern.Create(LexerPattern.FromPlainText("?")))
                .Then(MinOrMorePattern.Create(
                    0,
                    SequencePattern.Create()
                        .Then(TerminalPattern.Create(LexerPattern.FromRegex("\\w+")))
                        .Then(TerminalPattern.Create(LexerPattern.FromPlainText(":")))
                ))
                .Then(TerminalPattern.Create(LexerPattern.FromRegex("\\w+")));

            BuildParser();
            var result = pattern.Parse(ctx);
        }
    }
}
