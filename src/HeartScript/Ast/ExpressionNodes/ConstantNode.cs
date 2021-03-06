using System;

namespace HeartScript.Ast.Nodes
{
    public class ConstantNode : AstNode
    {
        public object? Value { get; }

        public ConstantNode(object? value, Type type) : base(type, AstType.Default)
        {
            if (value != null && value.GetType() != type)
                throw new ArgumentException("Type mismatch");

            Value = value;
        }
    }

    public partial class AstNode
    {
        public static ConstantNode Constant(object? value, Type type) => new ConstantNode(value, type);
        public static ConstantNode Constant(object value) => new ConstantNode(value, value.GetType());
    }
}
