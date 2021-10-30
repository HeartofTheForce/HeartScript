using System;

namespace HeartScript.Ast.Nodes
{
    public class AstNode
    {
        public Type Type { get; }

        public AstNode(Type type)
        {
            Type = type;
        }
    }
}
