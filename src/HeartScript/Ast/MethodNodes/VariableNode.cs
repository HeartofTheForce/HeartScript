using System;

namespace HeartScript.Ast.Nodes
{
    public class VariableNode : AstNode
    {
        public int Index { get; }

        public VariableNode(int index, Type type) : base(type, AstType.Variable)
        {
            Index = index;
        }
    }

    public partial class AstNode
    {
        public static VariableNode Variable(int index, Type type) => new VariableNode(index, type);
    }
}
