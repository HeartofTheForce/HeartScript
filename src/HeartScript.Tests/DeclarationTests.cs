using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class DeclarationTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //Assign
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int a;
                    a = 1;
                    return a;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 1
            },
            //Declaration Assign
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int a = 1;
                    return a;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 1
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
