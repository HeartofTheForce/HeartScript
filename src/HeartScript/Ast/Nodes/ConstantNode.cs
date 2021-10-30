using System;

namespace HeartScript.Ast.Nodes
{
    public class ConstantNode : AstNode
    {
        public object? Value { get; }

        public ConstantNode(object? value, Type type) : base(type, AstType.Constant)
        {
            if (value != null && value.GetType() != type)
                throw new ArgumentException("Type mismatch");

            Value = value;
        }
    }
}
