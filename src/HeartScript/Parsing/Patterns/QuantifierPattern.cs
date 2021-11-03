using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Parsing.Patterns
{
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

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var output = new List<IParseNode>();
            while (_max == null || output.Count < _max)
            {
                var result = _pattern.TryMatch(parser, ctx);

                if (result == null)
                    break;

                if (_max == null && localOffset == ctx.Offset)
                    throw new ZeroLengthMatchException(ctx.Offset);

                output.Add(result);
            }

            if (output.Count >= _min)
                return new QuantifierNode(localOffset, output);
            else
                return null;
        }
    }

    public class QuantifierNode : IParseNode
    {
        public int TextOffset { get; }
        public List<IParseNode> Children { get; }

        public QuantifierNode(int textOffset, List<IParseNode> children)
        {
            TextOffset = textOffset;
            Children = children;
        }
    }

}
