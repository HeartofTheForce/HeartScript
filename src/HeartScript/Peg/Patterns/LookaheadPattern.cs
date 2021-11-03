using HeartScript.Parsing;

namespace HeartScript.Peg.Patterns
{
    public class LookaheadPattern : IPattern
    {
        private readonly IPattern _pattern;
        private readonly bool _mode;

        private LookaheadPattern(IPattern pattern, bool mode)
        {
            _pattern = pattern;
            _mode = mode;
        }

        public static LookaheadPattern Positive(IPattern pattern)
        {
            return new LookaheadPattern(pattern, true);
        }

        public static LookaheadPattern Negative(IPattern pattern)
        {
            return new LookaheadPattern(pattern, false);
        }

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;
            var result = _pattern.Match(parser, ctx);
            ctx.Offset = localOffset;

            bool successful = result != null;
            if (_mode == successful)
                return new LookaheadNode(ctx.Offset, result);

            return null;
        }
    }

    public class LookaheadNode : IParseNode
    {
        public int TextOffset { get; }
        public IParseNode? Node { get; }

        public LookaheadNode(int textOffset, IParseNode? node)
        {
            TextOffset = textOffset;
            Node = node;
        }
    }
}
