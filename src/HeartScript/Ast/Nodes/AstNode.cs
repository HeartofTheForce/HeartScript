using System;

namespace HeartScript.Ast.Nodes
{
    public partial class AstNode
    {
        public Type Type { get; }
        public AstType NodeType { get; }

        public AstNode(Type type, AstType nodeType)
        {
            Type = type;
            NodeType = nodeType;
        }
    }
}
