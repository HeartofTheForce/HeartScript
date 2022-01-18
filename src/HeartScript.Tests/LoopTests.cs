using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class LoopTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //ForLoop
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    for(int i = 0; i < 5; i++)
                    {
                        r++;
                    }

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 5
            },
            //ForLoopNone
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    for(int i = 0; i < 0; i++)
                    {
                        r++;
                    }

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 0
            },
            //ForLoopSameIntialize
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;

                    for(int i = 0; i < 5; i++)
                    {
                        r++;
                    }

                    for(int i = 0; i < 5; i++)
                    {
                        r++;
                    }

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 10
            },
            //WhileLoop
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i = 0;
                    while(i < 5)
                    {
                        i++;
                    }

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 5
            },
            //WhileLoopNone
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i = 0;
                    while(i < 0)
                    {
                        i++;
                    }

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 0
            },
            //DoWhileLoop
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i = 0;
                    do
                    {
                        i++;
                    }
                    while(i < 5);

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 5
            },
            //DoWhileLoopAtLeastOnce
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i = 0;
                    do
                    {
                        i++;
                    }
                    while(i < 0);

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 1
            },
            //DoWhileLoopReturn
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    do
                    {
                        return 0;
                    }
                    while(true);
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 0
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
