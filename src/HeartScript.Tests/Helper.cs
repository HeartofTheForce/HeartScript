using HeartScript.Parsing;
using HeartScript.Peg;

namespace HeartScript.Tests
{
    public static class Helper
    {
        public static readonly PatternParser Parser;

        static Helper()
        {
            Parser = PegHelper.BuildPatternParser("./test.peg");
        }
    }
}
