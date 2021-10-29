using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Peg.Patterns
{
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

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
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

    public class ChoiceNode : IParseNode
    {
        public int CharIndex { get; }
        public int ChoiceIndex { get; }
        public IParseNode Node { get; }

        public ChoiceNode(int choiceIndex, IParseNode node)
        {
            ChoiceIndex = choiceIndex;
            Node = node;
        }
    }
}
