using HeartScript.Parsing.Nodes;

namespace HeartScript.Parsing
{
    public class LabelPattern : IPattern
    {
        private readonly string _label;
        private readonly IPattern _pattern;

        private LabelPattern(string label, IPattern pattern)
        {
            _label = label;
            _pattern = pattern;
        }

        public static LabelPattern Create(string label, IPattern pattern)
        {
            return new LabelPattern(label, pattern);
        }

        public IParseNode? Match(PatternParser parser, ParserContext ctx)
        {
            var result = _pattern.TryMatch(parser, ctx);

            if (result != null)
                return new LabelNode(_label, result);

            return null;
        }
    }
}
