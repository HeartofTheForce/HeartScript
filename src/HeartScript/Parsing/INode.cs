using System.Collections.Generic;

namespace HeartScript.Parsing
{
    public interface INode
    {
        string? Name { get; }
        string Value { get; }
        List<INode> Children { get; }
        int CharIndex { get; }
    }
}
