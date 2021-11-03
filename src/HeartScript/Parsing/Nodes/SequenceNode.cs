using System.Collections.Generic;

namespace HeartScript.Parsing.Nodes
{
    public class SequenceNode : IParseNode
    {
        public int TextOffset { get; }
        public List<IParseNode> Children { get; }

        public SequenceNode(int textOffset, List<IParseNode> children)
        {
            TextOffset = textOffset;
            Children = children;
        }
    }
}
