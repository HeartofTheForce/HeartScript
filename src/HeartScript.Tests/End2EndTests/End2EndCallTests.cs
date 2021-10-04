using NUnit.Framework;

namespace HeartScript.Tests.End2EndTests
{
    [TestFixture]
    public class End2EndCallTests
    {
        static readonly End2EndTestCase[] s_testCases = new End2EndTestCase[]
        {
            //Call
            new End2EndTestCase()
            {
                Infix = "sin(a) + cos(b)",
                ExpectedNodeString = "(+ ($ sin a) ($ cos b))",
            },
            //CallExpressionParameter
            new End2EndTestCase()
            {
                Infix = "tan(b + c)",
                ExpectedNodeString = "($ tan (+ b c))",
            },
            //CallChained
            new End2EndTestCase()
            {
                Infix = "sin(cos(b))",
                ExpectedNodeString = "($ sin ($ cos b))",
            },
            //CallMultiParameter
            new End2EndTestCase()
            {
                Infix = "max(a,b) + min(b,c)",
                ExpectedNodeString = "(+ ($ max a b) ($ min b c))",
            },
            //CallMultiExpressionParameter
            new End2EndTestCase()
            {
                Infix = "max(a + b, b + c)",
                ExpectedNodeString = "($ max (+ a b) (+ b c))",
            },
            //CallNestedMultiExpressionParameter
            new End2EndTestCase()
            {
                Infix = "max((a + b) * 2, (b / c))",
                ExpectedNodeString = "($ max (* (+ a b) 2) (/ b c))",
            },
            //CallChainedMultiParameter
            new End2EndTestCase()
            {
                Infix = "max(min(c, b), max(c, a))",
                ExpectedNodeString = "($ max ($ min c b) ($ max c a))",
            },
            //CallChainedMultiParameterUnary
            new End2EndTestCase()
            {
                Infix = "max(min(c, b), -a)",
                ExpectedNodeString = "($ max ($ min c b) (- a))",
            },
            //CallNestedExpressionParameter
            new End2EndTestCase()
            {
                Infix = "sin(min(c, b) - a)",
                ExpectedNodeString = "($ sin (- ($ min c b) a))",
            },
            //PostfixInfixUnary
            new End2EndTestCase()
            {
                Infix = "clamp(a,b,c) + - d",
                ExpectedNodeString = "(+ ($ clamp a b c) (- d))",
            },
            //Postfix
            new End2EndTestCase()
            {
                Infix = "clamp(a + b, b - c, c * d)",
                ExpectedNodeString = "($ clamp (+ a b) (- b c) (* c d))",
            },
            //UnaryCall
            new End2EndTestCase()
            {
                Infix = "-max(2, b)",
                ExpectedNodeString = "(- ($ max 2 b))",
            },
            //EmptyCall
            new End2EndTestCase()
            {
                Infix = "max()",
                ExpectedNodeString = "($ max)",
            },
        };

        [TestCaseSource(nameof(s_testCases))]
        public void TestCases(End2EndTestCase testCase)
        {
            testCase.Execute(Helper.TestOperators);
        }
    }
}
