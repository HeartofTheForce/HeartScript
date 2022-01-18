using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class IfElseTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //IfTrueReturn
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    if(true)
                        return 1;

                    return 0;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 1
            },
            //IfFalseReturn
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    if(false)
                        return 1;

                    return 0;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 0
            },
            //IfElseTrueReturn
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    if(true)
                        return 1;
                    else
                        return 0;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 1
            },
            //IfElseFalseReturn
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    if(false)
                        return 1;
                    else
                        return 0;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 0
            },
            //IfTrueSet
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i = 0;
                    if(true)
                        i = 1;

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 1
            },
            //IfFalseSet
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i = 0;
                    if(false)
                        i = 1;

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 0
            },
            //IfElseTrueSet
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i;
                    if(true)
                        i = 1;
                    else
                        i = 0;

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 1
            },
            //IfElseFalseSet
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int i;
                    if(false)
                        i = 1;
                    else
                        i = 0;

                    return i;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 0
            },
            //IfElseChainedFalse
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    if(false)
                    if(false)
                        return 1;
                    else
                        return 2;

                    return 3;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 3
            },
            //IfElseChainedTrue
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    if(true)
                    if(false)
                        return 1;
                    else
                        return 2;
                }
                ",
                Paramaters = System.Array.Empty<object>(),
                ExpectedResult = 2
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
