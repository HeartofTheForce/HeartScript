using System.Collections.Generic;
using HeartScript.Parsing;
using HeartScript.Peg.Nodes;

namespace HeartScript.Peg.Patterns
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
}
