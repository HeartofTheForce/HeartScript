using HeartScript.Expressions;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Tests.ExpressionPatternTests
{
    public interface IExpressionTestCase
    {
        void Execute(PatternParser parser);
    }

    public class ExpressionTestCase : IExpressionTestCase
    {
        public string Infix { get; set; }
        public string ExpectedOutput { get; set; }

        public void Execute(PatternParser parser)
        {
            var ctx = new ParserContext(Infix);

            var node = ExpressionPattern.Parse(parser, ctx);
            Assert.AreEqual(ExpectedOutput, node.ToString());
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }

    public class ExpressionTermTestCase : IExpressionTestCase
    {
        public string Infix { get; set; }
        public int ExpectedTextOffset { get; set; }

        public void Execute(PatternParser parser)
        {
            var ctx = new ParserContext(Infix);
            var ex = Assert.Throws<ExpressionTermException>(() => ExpressionPattern.Parse(parser, ctx));

            Assert.AreEqual(ExpectedTextOffset, ex.TextOffset);
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }

    public class UnexpectedTokenTestCase : IExpressionTestCase
    {
        public string Infix { get; set; }
        public int ExpectedTextOffset { get; set; }
        public string ExpectedPattern { get; set; }

        public void Execute(PatternParser parser)
        {
            var ctx = new ParserContext(Infix);
            var ex = Assert.Throws<UnexpectedTokenException>(() => ExpressionPattern.Parse(parser, ctx));

            Assert.AreEqual(ExpectedTextOffset, ex.TextOffset);
            Assert.AreEqual(ExpectedPattern, ex.ExpectedPattern);
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }
}
