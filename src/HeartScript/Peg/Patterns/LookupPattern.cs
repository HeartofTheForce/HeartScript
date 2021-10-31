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
                return new LookupNode(_key, result);

            return null;
        }
    }

    public class LookupNode : IParseNode
    {
        public int CharIndex { get; }
        public string Key { get; }
        public IParseNode Node { get; }

        public LookupNode(string key, IParseNode node)
        {
            Key = key;
            Node = node;
        }
    }
}
