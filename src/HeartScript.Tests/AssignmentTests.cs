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
            new CompilerExceptionTestCase<ArgumentException>()
            {
                Method = @"
                int main()
                {
                    int a;
                    int b = a = 1;
                }",
                Message = $"Cannot convert, {typeof(void)} to {typeof(int)}"
            }
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
