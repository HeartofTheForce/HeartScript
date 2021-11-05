using System;

namespace HeartScript.Ast.Nodes
{
    public class MethodInfoNode : AstNode
    {
        public string Name { get; }
        public Type ReturnType { get; }
        public Type[] ParameterTypes { get; }
        public AstNode Body { get; }

        public MethodInfoNode(string name, Type[] parameterTypes, AstNode body) : base(typeof(void), AstType.MethodDeclaration)
        {
            Name = name;
            ReturnType = body.Type;
            ParameterTypes = parameterTypes;
            Body = body;
        }
    }
}
