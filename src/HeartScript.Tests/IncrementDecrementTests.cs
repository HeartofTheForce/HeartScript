using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class IncrementDecrementTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //Post Increment Statement
            new CompilerTestCase()
            {
                Method = @"
                int main()
                {
                    int i = 1;
                    i++;
                    return i;
                }",
                ExpectedResult = 2,
            },
            //Post Increment Expression
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    double i = 1;
                    return i++;
                }",
                ExpectedResult = 1,
            },
            //Post Decrement Statement
            new CompilerTestCase()
            {
                Method = @"
                int main()
                {
                    int i = 1;
                    i--;
                    return i;
                }",
                ExpectedResult = 0,
            },
            //Post Decrement Expression
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i = 1;
                    return i--;
                }",
                ExpectedResult = 1,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
