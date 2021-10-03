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

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var output = new List<INode>();
            foreach (var pattern in _patterns)
            {
                var result = parser.TryMatch(pattern, ctx);

                if (result != null)
                    output.Add(result);
                else
                    return null;
            }

            return new PegNode(localOffset, output);
        }
    }

    public class ChoiceNode : PegNode
    {
        public int ChoiceIndex { get; }
        public INode Node => Children[0];

        public ChoiceNode(int choiceIndex, INode node) : base(node)
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

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            for (int i = 0; i < _patterns.Count; i++)
            {
                var result = parser.TryMatch(_patterns[i], ctx);

                if (result != null)
                    return new ChoiceNode(i, result);
            }

            return null;
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

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var output = new List<INode>();
            while (_max == null || output.Count < _max)
            {
                var result = parser.TryMatch(_pattern, ctx);

                if (result != null)
                    output.Add(result);
                else
                    break;
            }

            if (output.Count >= _min)
                return new PegNode(localOffset, output);
            else
                return null;
        }
    }

    public class KeyNode : PegNode
    {
        public string Key { get; }
        public INode Node => Children[0];

        public KeyNode(string key, INode node) : base(node)
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

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            var result = parser.TryMatch(parser.Patterns[_key], ctx);

            if (result != null)
                return new KeyNode(_key, result);
            else
                return null;
        }
    }
}
