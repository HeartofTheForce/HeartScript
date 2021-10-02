using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Tests
{
    public static class Helper
    {
        public static readonly IEnumerable<OperatorPattern> TestOperators;

        static Helper()
        {
            TestOperators = OperatorPatternBuilder.Parse("./test.ops");
        }
    }
}
