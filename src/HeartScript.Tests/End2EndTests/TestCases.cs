using HeartScript.Expressions;
using HeartScript.Parsing;
using NUnit.Framework;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Tests.End2EndTests
{
    public class End2EndTestCase
    {
        public string Infix { get; set; }
        public string ExpectedNodeString { get; set; }

        public void Execute(PatternParser parser)
        {
            var ctx = new ParserContext(Infix);

            var node = ExpressionPattern.Parse(parser, ctx);
            Assert.AreEqual(ExpectedNodeString, node.ToString());
        }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }
}
