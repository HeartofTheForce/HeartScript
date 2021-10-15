using System;
using System.Collections.Generic;
using HeartScript.Compiling;
using HeartScript.Expressions;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Tests.CompilerTests
{
    public class DemoContext
    {
        public int IntA { get; set; }
        public int IntB { get; set; }
        public bool BoolA { get; set; }
        public bool BoolB { get; set; }
        public double DoubleA { get; set; }
        public double DoubleB { get; set; }
    }

    public interface IExpressionCompilerTestCase
    {
        void Execute(IEnumerable<OperatorInfo> operators);
    }

    public class ExpressionCompilerTestCase<T> : IExpressionCompilerTestCase
    {
        public string Infix { get; set; }
        public string ExpectedString { get; set; }
        public Func<T> ExpectedExpression { get; set; }

        public void Execute(IEnumerable<OperatorInfo> operators)
        {
            var ctx = new ParserContext(Infix);

            var node = ExpressionPattern.Parse(operators, ctx);
            Assert.AreEqual(ExpectedString, node.ToString());

            var expectedResult = ExpectedExpression();
            var compiledExpression = ExpressionNodeCompiler.CompileFunction<T>((ExpressionNode)node);
            var actualResult = compiledExpression();
            Assert.AreEqual(expectedResult, actualResult);
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }

    public class ExpressionCompilerTestCase<TIn, TOut> : IExpressionCompilerTestCase
    {
        public string Infix { get; set; }
        public TIn Input { get; set; }
        public string ExpectedString { get; set; }
        public Func<TIn, TOut> ExpectedExpression { get; set; }

        public void Execute(IEnumerable<OperatorInfo> operators)
        {
            var ctx = new ParserContext(Infix);

            var node = ExpressionPattern.Parse(operators, ctx);
            Assert.AreEqual(ExpectedString, node.ToString());

            var expectedResult = ExpectedExpression(Input);
            var compiledExpression = ExpressionNodeCompiler.CompileFunction<TIn, TOut>((ExpressionNode)node);
            var actualResult = compiledExpression(Input);
            Assert.AreEqual(expectedResult, actualResult);
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }
}
