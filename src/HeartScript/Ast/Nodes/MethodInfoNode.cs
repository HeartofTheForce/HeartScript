using System;
using System.Collections.Generic;

namespace HeartScript.Ast.Nodes
{
    public class MethodInfoNode : AstNode
    {
        public string Name { get; }
        public Type ReturnType { get; }
        public Type[] ParameterTypes { get; }
        public List<AstNode> Statements { get; }

        public MethodInfoNode(string name, Type returnType, Type[] parameterTypes) : base(typeof(void), AstType.Default)
        {
            Name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
            Statements = new List<AstNode>();
        }
    }
}
