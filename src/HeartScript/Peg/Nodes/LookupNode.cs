using HeartScript.Parsing;

namespace HeartScript.Peg.Nodes
{
    public class LookupNode : PegNode
    {
        public INode Node => Children[0];

        public LookupNode(string key, INode node) : base(node)
        {
            Name = key;
        }
    }
}
