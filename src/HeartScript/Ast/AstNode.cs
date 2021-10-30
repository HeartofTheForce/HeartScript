using System;

namespace HeartScript.Ast
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
