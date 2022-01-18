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
                ExpectedResult = 5,
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
                ExpectedResult = 0,
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
                ExpectedResult = 10,
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
                ExpectedResult = 5,
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
                ExpectedResult = 0,
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
                ExpectedResult = 5,
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
                ExpectedResult = 1,
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
                ExpectedResult = 0,
            },
            //ForLoopBreak
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    for(int i = 0; i < 5; i++)
                    {
                        if(i == 2)
                            break;

                        r++;
                    }

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 2,
            },
            //WhileLoopBreak
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    while(true)
                    {
                        if(r == 2)
                            break;

                        r++;
                    }

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 2,
            },
            //DoWhileLoopBreak
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    do
                    {
                        if(r == 2)
                            break;

                        r++;
                    }
                    while(true);

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 2,
            },
            //ForLoopContinue
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    for(int i = 0; i < 5; i++)
                    {
                        if(i < 1)
                            continue;

                        r++;
                    }

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 4,
            },
            //WhileLoopContinue
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    int i = 0;
                    while(i < 5)
                    {
                        if(i++ < 1)
                            continue;

                        r++;
                    }

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 4,
            },
            //DoWhileLoopContinue
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int r = 0;
                    int i = 0;
                    do
                    {
                        if(i++ < 1)
                            continue;

                        r++;
                    }
                    while(i < 5);

                    return r;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 4,
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
