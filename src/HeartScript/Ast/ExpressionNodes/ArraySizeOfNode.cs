using System;

namespace HeartScript.Ast.Nodes
{
    public class ArraySizeOfNode : AstNode
    {
        public AstNode Array { get; }

        public ArraySizeOfNode(AstNode array) : base(typeof(int), AstType.Default)
        {
            if (!array.Type.IsArray)
                throw new ArgumentException($"{nameof(array)} must be an array");

            Array = array;
        }
    }

    public partial class AstNode
    {
        public static ArraySizeOfNode ArraySizeOf(AstNode array) => new ArraySizeOfNode(array);
    }
}
