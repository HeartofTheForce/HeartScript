using System;

namespace HeartScript.Ast.Nodes
{
    public class ParameterNode : AstNode
    {
        public int ParameterIndex { get; }

        public ParameterNode(int parameterIndex, Type type) : base(type, AstType.Parameter)
        {
            ParameterIndex = parameterIndex;
        }
    }

    public partial class AstNode
    {
        public static ParameterNode Parameter(int parameterIndex, Type type) => new ParameterNode(parameterIndex, type);
    }
}
