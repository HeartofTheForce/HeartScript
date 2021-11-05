using System;

namespace HeartScript.Ast.Nodes
{
    public class MethodInfoNode : AstNode
    {
        public string Name { get; }
        public Type ReturnType { get; }
        public Type[] ParameterTypes { get; }
        public BlockNode Body { get; }

        public MethodInfoNode(string name, Type[] parameterTypes, BlockNode body) : base(typeof(void), AstType.Default)
        {
            Name = name;
            ReturnType = body.Type;
            ParameterTypes = parameterTypes;
            Body = body;
        }
    }
}
