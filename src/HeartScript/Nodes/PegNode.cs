using System.Collections.Generic;

namespace HeartScript.Nodes
{
    public class PegNode : INode
    {
        public string? Value { get; }
        public List<INode>? Children { get; }

        public PegNode(List<INode> children)
        {
            Value = null;
            Children = children;
        }

        public PegNode(INode child)
        {
            Value = null;
            Children = new List<INode> { child };
        }

        public PegNode(string value)
        {
            Value = value;
            Children = null;
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
