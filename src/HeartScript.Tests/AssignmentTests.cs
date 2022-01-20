using System;
using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class AssignmentTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //Parameter Assign
            new CompilerTestCase()
            {
                Method = @"
                double main(int a)
                {
                    a = 1;
                    return a;
                }",
                Paramaters = new object[]
                {
                    0,
                },
                ExpectedResult = 1
            },
            //Variable Implicit Convert Assign
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    double a;
                    a = 1;
                    return a;
                }",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 1
            },
            //Variable Implicit Convert Declaration Assign
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    double a = 1;
                    return a;
                }",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 1
            },
            //Variable Implicit Convert Declaration Assign
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    double a = 1;
                    return a;
                }",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 1
            },
            //Chained Assign
            new CompilerTestCase()
            {
                Method = @"
                int main()
                {
                    int a;
                    int b;
                    b = a = 1;
                    return b;
                }",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 1
            },
            //Chained Declaration Assign
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int a;
                    int b = a = 1;
                    return b;
                }",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 1
            },
            //Chained Implicit Convert Assign
            new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int a;
                    double b = a = 1;
                    return b;
                }",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 1
            },
            //Type Mistmatch Chained Implicit Convert Assign
            new CompilerExceptionTestCase<ArgumentException>()
            {
                Method = @"
                int main()
                {
                    double a;
                    int b = a = 1;
                    return b;
                }",
                Message = $"Cannot convert, {typeof(double)} to {typeof(int)}"
            },
            //Cache Assign Edge Case
            //Overlapping temp variable usage (i = 1 & i = 0)
            new CompilerTestCase()
            {
                Method = @"
                int main()
                {
                    int[] arr = new int[2];
                    int i;
                    int j;
                    j = arr[i = 1] = i = 0;

                    return arr[1];
                }",
                Paramaters = Array.Empty<object>(),
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
