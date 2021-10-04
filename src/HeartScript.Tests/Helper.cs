using System.Collections.Generic;
using HeartScript.Expressions;

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
