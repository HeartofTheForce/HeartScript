namespace HeartScript.Parsing.Nodes
{
    public class PredicateNode : IParseNode
    {
        public int TextOffset { get; }
        public IParseNode? Node { get; }

        public PredicateNode(int textOffset, IParseNode? node)
        {
            TextOffset = textOffset;
            Node = node;
        }
    }
}
