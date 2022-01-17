using System;

namespace HeartScript.Ast.Nodes
{
    public class ParameterNode : AstNode
    {
        public short Index { get; }

        public ParameterNode(int index, Type type) : base(type, AstType.Default)
        {
            Index = (short)index;
        }
    }

    public partial class AstNode
    {
        public static ParameterNode Parameter(int index, Type type) => new ParameterNode(index, type);
    }
}
