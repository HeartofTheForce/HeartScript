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

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            var result = parser.TryMatch(parser.Patterns[_key], ctx);

            if (result != null)
            {
                result.Name = _key;
                return result;
            }

            return null;
        }
    }
}
