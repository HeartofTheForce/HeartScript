using HeartScript.Peg.Patterns;

namespace HeartScript.Parsing
{
    public interface IPattern
    {
        IParseNode? Match(PatternParser parser, ParserContext ctx);
    }

    public static class PatternExtensions
    {
        public static IParseNode? TryMatch(this IPattern pattern, PatternParser parser, ParserContext ctx)
        {
            int localOffset = ctx.Offset;

            var result = pattern.Match(parser, ctx);
            if (result == null)
                ctx.Offset = localOffset;

            return result;
        }

        public static IPattern Trim(this IPattern pattern, IPattern trimPattern)
        {
            return SequencePattern.Create()
                .Discard(trimPattern)
                .Then(pattern)
                .Discard(trimPattern);
        }
    }
}
