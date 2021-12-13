using Heart.Parsing;

namespace HeartScript.Tests
{
    public static class Utility
    {
        public static readonly PatternParser Parser = ParsingHelper.BuildPatternParser("./test.hg");
    }
}
