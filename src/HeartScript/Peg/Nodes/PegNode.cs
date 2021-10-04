using System.Collections.Generic;
using HeartScript.Parsing;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Peg.Nodes
{
    public class PegNode : INode
    {
        public string Value { get; }
        public List<INode> Children { get; }
        public int CharIndex { get; set; }

        public PegNode(int charIndex, List<INode> children)
        {
            Value = null;
            Children = children;
            CharIndex = charIndex;
        }

        public PegNode(INode child)
        {
            Value = null;
            Children = new List<INode> { child };
            CharIndex = Children[0].CharIndex;
        }

        public PegNode(int charIndex, string value)
        {
            Value = value;
            Children = null;
            CharIndex = charIndex;
        }

        public override string ToString()
        {
            if (Value != null)
                return Value;

            if (Children != null)
                return string.Join("", Children);

            throw new System.Exception();
        }
    }
}
