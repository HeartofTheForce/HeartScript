using System;
using System.Collections.Generic;
using HeartScript.Parsing;
using HeartScript.Peg.Nodes;

namespace HeartScript.Peg.Patterns
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
}
