using System;

namespace HeartScript.Ast.Nodes
{
    public class ParameterNode : AstNode
    {
        public ParameterNode(Type type) : base(type, AstType.Parameter)
        {
        }
    }

    public partial class AstNode
    {
        public static ParameterNode Parameter(Type type) => new ParameterNode(type);
    }
}
