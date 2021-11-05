using HeartScript.Parsing;

namespace HeartScript.Peg.Patterns
{
    public class PredicatePattern : IPattern
    {
        private readonly IPattern _pattern;
        private readonly bool _mode;

        private PredicatePattern(IPattern pattern, bool mode)
        {
            _pattern = pattern;
            _mode = mode;
        }

        public static PredicatePattern Positive(IPattern pattern)
        {
            return new PredicatePattern(pattern, true);
        }

        public static PredicatePattern Negative(IPattern pattern)
        {
            return new PredicatePattern(pattern, false);
        }

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;
            var result = _pattern.Match(parser, ctx);
            ctx.Offset = localOffset;

            bool successful = result != null;
            if (_mode == successful)
                return new PredicateNode(ctx.Offset, result);

            return null;
        }
    }

    public class PredicateNode : IParseNode
    {
        public int TextOffset { get; }
        public IParseNode? Node { get; }

        public PredicateNode(int textOffset, IParseNode? node)
        {
            TextOffset = textOffset;
            Node = node;
        }
    }
}
