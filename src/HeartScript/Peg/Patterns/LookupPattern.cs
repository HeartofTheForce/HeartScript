using HeartScript.Parsing;

namespace HeartScript.Peg.Patterns
{
    public class LookupPattern : IPattern
    {
        private readonly string _key;

        private LookupPattern(string key)
        {
            _key = key;
        }

        public static LookupPattern Create(string key)
        {
            return new LookupPattern(key);
        }

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            var result = parser.Patterns[_key].TryMatch(parser, ctx);

            if (result != null)
                return result;

            return null;
        }
    }
}
