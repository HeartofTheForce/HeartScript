using System;
using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class CallTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //Call Local
            new CompilerTestCase()
            {
                Method = @"
                int min(int a, int b) => a < b ? a : b;
                int main()
                {
                    return min(1, 2);
                }",
                ExpectedResult = 1,
            },
            //Call On Type
            new CompilerTestCase()
            {
                Method = @"
                int main()
                {
                    return Math.Min(1, 2);
                }",
                ExpectedResult = 1,
            },
            //Call Overload A
            new CompilerTestCase()
            {
                Method = @"
                double test(double a, double b) => 1;
                double test(int a, double b) => 2;
                double main()
                {
                    return test(0.0, 0.0);
                }",
                ExpectedResult = 1.0,
            },
            //Call Overload B
            new CompilerTestCase()
            {
                Method = @"
                double test(double a, double b) => 1;
                double test(int a, double b) => 2;
                double main()
                {
                    return test(0, 0.0);
                }",
                ExpectedResult = 2.0,
            },
            new CompilerExceptionTestCase<Exception>()
            {
                Method = @"
                double test(double a, int b) => 1;
                double test(int a, double b) => 2;
                double main()
                {
                    return test(0, 0);
                }",
                Message = $"The call is ambiguous between the following methods: 'test({typeof(double).FullName}, {typeof(int).FullName})' and 'test({typeof(int).FullName}, {typeof(double).FullName})'",
            }
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(ICompilerTestCase testCase)
        {
            testCase.Execute();
        }
    }
}
