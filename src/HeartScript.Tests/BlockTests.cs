using System;
using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class BlockTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //Seperate Blocks
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int a;
                    {
                        int x = 1;
                        a = x;
                    }

                    int c;
                    {
                        int x = 2;
                        c = x;
                    }

                    return a + c;
                }
                ",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 3
            },
            //Nearest Scope
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int a;
                    int x = 1;
                    {
                        int x = 2;
                        a = x;
                    }

                    return a + x;
                }
                ",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 3
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
