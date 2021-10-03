using System.Collections.Generic;

namespace HeartScript.Nodes
{
    public interface INode
    {
        string Value { get; }
        List<INode> Children { get; }
        int CharIndex { get; }
    }
}
