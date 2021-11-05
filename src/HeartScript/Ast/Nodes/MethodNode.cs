using System;

namespace HeartScript.Ast.Nodes
{
    public class MethodNode : AstNode
    {
        public string Name { get; }
        public Type ReturnType { get; }
        public Type[] ParameterTypes { get; }
        public AstNode Body { get; }

        public MethodNode(string name, Type[] parameterTypes, AstNode body) : base(typeof(void), AstType.Method)
        {
            Name = name;
            ReturnType = body.Type;
            ParameterTypes = parameterTypes;
            Body = body;
        }
    }
}
