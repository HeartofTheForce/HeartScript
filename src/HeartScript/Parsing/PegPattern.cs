using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class PegResult
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

    public interface IPegPattern
    {
        PegResult Parse(PegParserContext ctx, Lexer lexer);
    }

    public class TerminalPattern : IPegPattern
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

        public PegResult Parse(PegParserContext ctx, Lexer lexer)
        {
            if (lexer.TryEat(_lexerPattern, out var current))
                return PegResult.Success(current.CharIndex, new PegNode(current.Value));

            return PegResult.Error(lexer.Offset, $"Expected match @ {lexer.Offset}, {_lexerPattern}");
        }
    }

    public class SequencePattern : IPegPattern
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

        public PegResult Parse(PegParserContext ctx, Lexer lexer)
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

    public class ChoiceNode : PegNode
    {
        public int ChoiceIndex { get; }
        public INode ChoiceValue => Children[0];

        public ChoiceNode(int choiceIndex, INode choiceValue) : base(choiceValue)
        {
            ChoiceIndex = choiceIndex;
        }
    }

    public class ChoicePattern : IPegPattern
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

        public PegResult Parse(PegParserContext ctx, Lexer lexer)
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

    public class QuantifierPattern : IPegPattern
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

        public PegResult Parse(PegParserContext ctx, Lexer lexer)
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

    public class KeyNode : PegNode
    {
        public string Key { get; }
        public INode KeyValue => Children[0];

        public KeyNode(string key, INode keyValue) : base(keyValue)
        {
            Key = key;
        }
    }

    public class KeyPattern : IPegPattern
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

        public PegResult Parse(PegParserContext ctx, Lexer lexer)
        {
            var result = ctx.Patterns[_key].Parse(ctx, lexer);
            if (result.ErrorMessage != null)
                return result;

            if (result.Value != null)
                return PegResult.Success(result.CharIndex, new KeyNode(_key, result.Value));

            throw new Exception();
        }
    }
}
