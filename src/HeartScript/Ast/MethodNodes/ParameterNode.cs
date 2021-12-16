using System;

namespace HeartScript.Ast.Nodes
{
    public class ParameterNode : AstNode
    {
        public int Index { get; }

        public ParameterNode(int index, Type type) : base(type, AstType.Parameter)
        {
            Index = index;
        }
    }

    public partial class AstNode
    {
        public static ParameterNode Parameter(int index, Type type) => new ParameterNode(index, type);
    }
}
