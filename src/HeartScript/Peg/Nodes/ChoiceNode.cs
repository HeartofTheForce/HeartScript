using HeartScript.Parsing;

namespace HeartScript.Peg.Nodes
{
    public class ChoiceNode : PegNode
    {
        public int ChoiceIndex { get; }
        public INode Node => Children[0];

        public ChoiceNode(int choiceIndex, INode node) : base(node)
        {
            ChoiceIndex = choiceIndex;
        }
    }
}
