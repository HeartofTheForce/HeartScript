using System.Collections.Generic;
using HeartScript.Parsing;
using HeartScript.Peg.Nodes;

namespace HeartScript.Peg.Patterns
{
    public class SequencePattern : IPattern
    {
        private struct SequenceStep
        {
            public IPattern Pattern { get; set; }
            public bool Discard { get; set; }
        }

        private readonly List<SequenceStep> _steps;

        private SequencePattern()
        {
            _steps = new List<SequenceStep>();
        }

        public static SequencePattern Create()
        {
            return new SequencePattern();
        }

        public SequencePattern Then(IPattern pattern)
        {
            _steps.Add(new SequenceStep()
            {
                Pattern = pattern,
                Discard = false,
            });

            return this;
        }

        public SequencePattern Discard(IPattern pattern)
        {
            _steps.Add(new SequenceStep()
            {
                Pattern = pattern,
                Discard = true,
            });

            return this;
        }

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var output = new List<INode>();
            foreach (var step in _steps)
            {
                var result = parser.TryMatch(step.Pattern, ctx);

                if (result == null)
                    return null;

                if (!step.Discard)
                    output.Add(result);
            }

            return new PegNode(localOffset, output);
        }
    }
}
