namespace HeartScript.Parsing.Nodes
{
    public class LabelNode : IParseNode
    {
        public int TextOffset { get; }
        public string Label { get; }
        public IParseNode Node { get; }

        public LabelNode(string label, IParseNode node)
        {
            TextOffset = node.TextOffset;
            Label = label;
            Node = node;
        }
    }
}
