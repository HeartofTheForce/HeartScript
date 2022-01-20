using System;

namespace HeartScript.Ast.Nodes
{
    public class ArrayConstructorNode : AstNode
    {
        public AstNode Length { get; }

        public ArrayConstructorNode(Type type, AstNode length) : base(type, AstType.Default)
        {
            if (TypeHelper.IsIntegral(length.Type))
                throw new ArgumentException($"{nameof(length)} must be integral");

            Length = length;
        }
    }

    public partial class AstNode
    {
        public static ArrayConstructorNode ArrayConstructor(Type type, AstNode length) => new ArrayConstructorNode(type, length);
    }
}
