using HeartScript.Parsing;
using HeartScript.Peg;

namespace HeartScript.Tests
{
    public static class Helper
    {
        public static readonly PatternParser Parser = PegHelper.BuildPatternParser("./test.peg");
    }
}
