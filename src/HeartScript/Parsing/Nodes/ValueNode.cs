namespace HeartScript.Parsing.Nodes
{
    public class ValueNode : IParseNode
    {
        public int TextOffset { get; }
        public string Value { get; }

        public ValueNode(int textOffset, string value)
        {
            TextOffset = textOffset;
            Value = value;
        }
    }
}
