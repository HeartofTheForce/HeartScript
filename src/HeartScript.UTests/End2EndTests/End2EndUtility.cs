namespace HeartScript.UTests.End2EndTests
{
    public struct End2EndTestCase
    {
        public string Infix { get; set; }
        public string ExpectedNodeString { get; set; }

        public override string ToString()
        {
            return $"\"{Infix}\"";
        }
    }
}
