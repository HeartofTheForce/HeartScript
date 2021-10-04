using HeartScript.Parsing;
using HeartScript.Peg.Nodes;

namespace HeartScript.Peg.Patterns
{
    public class KeyPattern : IPattern
    {
        private readonly string _key;

        private KeyPattern(string key)
        {
            _key = key;
        }

        public static KeyPattern Create(string key)
        {
            return new KeyPattern(key);
        }

        public INode? Match(PatternParser parser, ParserContext ctx)
        {
            var result = parser.TryMatch(parser.Patterns[_key], ctx);

            if (result != null)
                return new KeyNode(_key, result);
            else
                return null;
        }
    }
}
