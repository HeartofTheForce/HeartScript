using System.Collections.Generic;
using HeartScript.Parsing;
using NUnit.Framework;

namespace HeartScript.Tests.AstParserTests
{
    public interface IExpressionTestCase
    {
        void Execute(IEnumerable<OperatorInfo> operators);
    }

    public class AstTestCase : IExpressionTestCase
    {
        public string Infix { get; set; }
        public string ExpectedOutput { get; set; }

        public void Execute(IEnumerable<OperatorInfo> operators)
        {
            var lexer = new Lexer(Infix);

            var node = AstParser.Parse(operators, lexer);
            Assert.AreEqual(ExpectedOutput, node.ToString());
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }

    public struct ExpressionTermTestCase : IExpressionTestCase
    {
        public string Infix { get; set; }
        public int ExpectedCharIndex { get; set; }

        public void Execute(IEnumerable<OperatorInfo> operators)
        {
            var lexer = new Lexer(Infix);
            var ex = Assert.Throws<ExpressionTermException>(() => AstParser.Parse(operators, lexer));

            Assert.AreEqual(ExpectedCharIndex, ex.CharIndex);
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }

    public struct UnexpectedTokenTestCase : IExpressionTestCase
    {
        public string Infix { get; set; }
        public int ExpectedCharIndex { get; set; }
        public string ExpectedPattern { get; set; }

        public void Execute(IEnumerable<OperatorInfo> operators)
        {
            var lexer = new Lexer(Infix);
            var ex = Assert.Throws<UnexpectedTokenException>(() => AstParser.Parse(operators, lexer));

            Assert.AreEqual(ExpectedCharIndex, ex.CharIndex);
            Assert.AreEqual(ExpectedPattern, ex.ExpectedPattern);
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }
}
