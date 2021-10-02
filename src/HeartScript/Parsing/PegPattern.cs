using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
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

        public PatternResult Match(Parser parser, ParserContext ctx)
        {
            int startIndex = 0;

            var output = new List<INode>();
            foreach (var pattern in _patterns)
            {
                var result = parser.TryMatch(pattern, ctx);

                if (result.ErrorMessage != null)
                    return result;

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

        public PatternResult Match(Parser parser, ParserContext ctx)
        {
            PatternResult? furthestResult = null;
            for (int i = 0; i < _patterns.Count; i++)
            {
                var result = parser.TryMatch(_patterns[i], ctx);

                if (result.Value != null)
                    return PatternResult.Success(result.CharIndex, new ChoiceNode(i, result.Value));

                if (furthestResult == null || result.CharIndex > furthestResult.CharIndex)
                    furthestResult = result;
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

        public PatternResult Match(Parser parser, ParserContext ctx)
        {
            int startIndex = ctx.Offset;

            PatternResult? result = null;
            var output = new List<INode>();
            while (_max == null || output.Count < _max)
            {
                result = parser.TryMatch(_pattern, ctx);

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

        public PatternResult Match(Parser parser, ParserContext ctx)
        {
            var result = parser.TryMatch(parser.Patterns[_key], ctx);
            if (result.ErrorMessage != null)
                return result;

            if (result.Value != null)
                return PatternResult.Success(result.CharIndex, new KeyNode(_key, result.Value));

            throw new Exception();
        }
    }
}
