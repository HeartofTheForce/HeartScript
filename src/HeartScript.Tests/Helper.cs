using HeartScript.Parsing;

namespace HeartScript.Tests
{
    public static class Helper
    {
        public static readonly PatternParser Parser;

        static Helper()
        {
            Parser = ParsingHelper.BuildPatternParser("./test.peg");
        }
    }
}
