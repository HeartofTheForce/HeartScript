using System;

namespace HeartScript.Ast.Nodes
{
    public class MethodInfoNode : AstNode
    {
        public string Name { get; }
        public Type ReturnType { get; }
        public Type[] ParameterTypes { get; }
        public VariableNode[] Variables { get; }
        public BlockNode Body { get; }

        public MethodInfoNode(
            string name,
            Type returnType,
            Type[] parameterTypes,
            VariableNode[] variables,
            BlockNode body)
        : base(typeof(void), AstType.Default)
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            Variables = variables;
            Body = body;
        }
    }
}
