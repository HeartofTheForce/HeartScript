using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Tests
{
    public static class Helper
    {
        public static readonly IEnumerable<OperatorInfo> TestOperators;

        static Helper()
        {
            TestOperators = OperatorInfoBuilder.Parse("./test.ops");
        }
    }
}
