using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class TerminalPattern : IPattern
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

        public PatternResult Parse(ParserContext ctx, Lexer lexer)
        {
            if (lexer.TryEat(_lexerPattern, out var current))
                return PatternResult.Success(current.CharIndex, new PegNode(current.Value));

            return PatternResult.Error(lexer.Offset, $"Expected match @ {lexer.Offset}, {_lexerPattern}");
        }
    }

    public class SequencePattern : IPattern
    {
        private readonly List<IPattern> _patterns;

        private SequencePattern()
        {
            _patterns = new List<IPattern>();
        }

        public static SequencePattern Create()
        {
            return new SequencePattern();
        }

        public SequencePattern Then(IPattern pattern)
        {
            _patterns.Add(pattern);
            return this;
        }

        public PatternResult Parse(ParserContext ctx, Lexer lexer)
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

            return PatternResult.Success(startIndex, new PegNode(output));
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

    public class ChoicePattern : IPattern
    {
        private readonly List<IPattern> _patterns;

        private ChoicePattern()
        {
            _patterns = new List<IPattern>();
        }

        public static ChoicePattern Create()
        {
            return new ChoicePattern();
        }

        public ChoicePattern Or(IPattern pattern)
        {
            _patterns.Add(pattern);
            return this;
        }

        public PatternResult Parse(ParserContext ctx, Lexer lexer)
        {
            int startIndex = lexer.Offset;

            PatternResult? furthestResult = null;
            for (int i = 0; i < _patterns.Count; i++)
            {
                var result = _patterns[i].Parse(ctx, lexer);

                if (result.Value != null)
                    return PatternResult.Success(result.CharIndex, new ChoiceNode(i, result.Value));

                if (furthestResult == null || result.CharIndex > furthestResult.CharIndex)
                    furthestResult = result;

                if (result.ErrorMessage != null)
                    lexer.Offset = startIndex;
            }

            return furthestResult!;
        }
    }

    public class QuantifierPattern : IPattern
    {
        private readonly int _min;
        private readonly int? _max;
        private readonly IPattern _pattern;

        private QuantifierPattern(int min, int? max, IPattern pattern)
        {
            if (max < min)
                throw new ArgumentException($"{nameof(max)} < {nameof(min)}");

            _min = min;
            _max = max;
            _pattern = pattern;
        }

        public static QuantifierPattern MinOrMore(int min, IPattern pattern)
        {
            return new QuantifierPattern(min, null, pattern);
        }

        public static QuantifierPattern Optional(IPattern pattern)
        {
            return new QuantifierPattern(0, 1, pattern);
        }

        public PatternResult Parse(ParserContext ctx, Lexer lexer)
        {
            int startIndex = lexer.Offset;

            PatternResult? result = null;
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
                return PatternResult.Success(startIndex, new PegNode(output));
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

    public class KeyPattern : IPattern
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

        public PatternResult Parse(ParserContext ctx, Lexer lexer)
        {
            var result = ctx.Patterns[_key].Parse(ctx, lexer);
            if (result.ErrorMessage != null)
                return result;

            if (result.Value != null)
                return PatternResult.Success(result.CharIndex, new KeyNode(_key, result.Value));

            throw new Exception();
        }
    }
}
