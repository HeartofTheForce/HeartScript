using System;

namespace HeartScript.Ast.Nodes
{
    public class ArrayLengthNode : AstNode
    {
        public AstNode Array { get; }

        public ArrayLengthNode(AstNode array) : base(typeof(int), AstType.Default)
        {
            if (!array.Type.IsArray)
                throw new ArgumentException($"{nameof(array)} must be an array");

            Array = array;
        }
    }

    public partial class AstNode
    {
        public static ArrayLengthNode ArrayLength(AstNode array) => new ArrayLengthNode(array);
    }
}
