namespace HeartScript.Tests.AstParserTests
{
    public class AstTestCase
    {
        public string Infix { get; set; }
        public string ExpectedOutput { get; set; }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }
}
