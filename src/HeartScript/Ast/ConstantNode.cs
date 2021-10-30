using System;

namespace HeartScript.Ast
{
    public class ConstantNode : AstNode
    {
        public object? Value { get; }

        public ConstantNode(object? value, Type type) : base(type)
        {
            if (value != null && value.GetType() != type)
                throw new ArgumentException("Type mismatch");

            Value = value;
        }
    }
}
