using System.Collections.Generic;

namespace HeartScript.Parsing.Nodes
{
    public class QuantifierNode : IParseNode
    {
        public int TextOffset { get; }
        public List<IParseNode> Children { get; }

        public QuantifierNode(int textOffset, List<IParseNode> children)
        {
            TextOffset = textOffset;
            Children = children;
        }
    }
}
