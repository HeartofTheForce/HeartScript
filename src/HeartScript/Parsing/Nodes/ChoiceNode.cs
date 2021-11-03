namespace HeartScript.Parsing.Nodes
{
    public class ChoiceNode : IParseNode
    {
        public int TextOffset { get; }
        public int ChoiceIndex { get; }
        public IParseNode Node { get; }

        public ChoiceNode(int choiceIndex, IParseNode node)
        {
            TextOffset = node.TextOffset;
            ChoiceIndex = choiceIndex;
            Node = node;
        }
    }
}
