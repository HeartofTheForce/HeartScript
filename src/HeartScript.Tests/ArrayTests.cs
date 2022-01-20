using System;
using NUnit.Framework;

namespace HeartScript.Tests.VariableTests
{
    [TestFixture]
    public class ArrayTests
    {
        private static readonly ICompilerTestCase[] s_testCases = new ICompilerTestCase[]
        {
            //Declare Array
             new CompilerTestCase()
            {
                Method = @"
                double main()
                {
                    int[] arr = new int[1];
                    return arr[0];
                }",
                Paramaters = Array.Empty<object>(),
                ExpectedResult = 0,
            },
            //Assign Array Index
            new CompilerTestCase()
            {
                Method = @"
                double main(int[] arr)
                {
                    arr[0] = 1;
                    return arr[0];
                }",
                Paramaters = new object[]{ new int[1] },
                ExpectedResult = 1,
            },
            //Post Increment Array Index
            new CompilerTestCase()
            {
                Method = @"
                double main(int[] arr)
                {
                    arr[0]++;
                    return arr[0];
                }",
                Paramaters = new object[]{ new int[1] },
                ExpectedResult = 1,
            },
            //Post Decrement Array Index
            new CompilerTestCase()
            {
                Method = @"
                double main(int[] arr)
                {
                    arr[0]++;
                    return arr[0];
                }",
                Paramaters = new object[]{ new int[1] },
                ExpectedResult = 1,
            },
            //Size Of Array
            new CompilerTestCase()
            {
                Method = @"
                double main(int[] arr)
                {
                    return sizeof(arr);
                }",
                Paramaters = new object[]{ new int[1] },
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
