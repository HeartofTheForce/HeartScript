using HeartScript.Parsing;

namespace HeartScript.Peg.Nodes
{
    public class LookupNode : PegNode
    {
        public INode Node => Children[0];

        public LookupNode(string name, INode node) : base(node)
        {
            Name = name;
        }
    }
}
