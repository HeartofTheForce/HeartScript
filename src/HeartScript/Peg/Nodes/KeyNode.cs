using HeartScript.Parsing;

namespace HeartScript.Peg.Nodes
{
    public class KeyNode : PegNode
    {
        public string Key { get; }
        public INode Node => Children[0];

        public KeyNode(string key, INode node) : base(node)
        {
            Key = key;
        }
    }
}
