using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Parsing.Patterns
{
    public class SequencePattern : IPattern
    {
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

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            if (_steps.Count <= 1)
                throw new Exception($"Expected > 1 {nameof(_steps)} found: {_steps.Count}");

            int localOffset = ctx.Offset;

            var output = new List<IParseNode>();
            foreach (var step in _steps)
            {
                var result = step.Pattern.TryMatch(parser, ctx);

                if (result == null)
                    return null;

                if (!step.Discard)
                    output.Add(result);
            }

            if (output.Count == 0)
                throw new Exception($"Cannot {nameof(Discard)} all steps");

            if (output.Count == 1)
                return output[0];
            else
                return new SequenceNode(localOffset, output);
        }

        private struct SequenceStep
        {
            public IPattern Pattern { get; set; }
            public bool Discard { get; set; }
        }
    }

    public class SequenceNode : IParseNode
    {
        public int TextOffset { get; }
        public List<IParseNode> Children { get; }

        public SequenceNode(int textOffset, List<IParseNode> children)
        {
            TextOffset = textOffset;
            Children = children;
        }
    }
}
