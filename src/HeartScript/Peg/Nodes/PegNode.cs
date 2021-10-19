using System;
using System.Collections.Generic;
using HeartScript.Parsing;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Peg.Nodes
{
    public class PegNode : INode
    {
        public string? Name { get; }
        public string Value { get; }
        public List<INode> Children { get; }
        public int CharIndex { get; set; }

        public PegNode(string? name, int charIndex, List<INode> children)
        {
            Name = name;
            Value = null;
            Children = children;
            CharIndex = charIndex;
        }

        public PegNode(string? name, INode child)
        {
            Name = name;
            Value = null;
            Children = new List<INode> { child };
            CharIndex = Children[0].CharIndex;
        }

        public PegNode(string? name, int charIndex, string value)
        {
            Name = name;
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

            throw new Exception();
        }
    }
}
