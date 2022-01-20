using System;

namespace HeartScript.Ast.Nodes
{
    public class ArrayIndexNode : AstNode
    {
        public AstNode Array { get; }
        public AstNode Index { get; }

        public ArrayIndexNode(AstNode array, AstNode index) : base(array.Type.GetElementType(), AstType.Default)
        {
            if (!array.Type.IsArray)
                throw new ArgumentException($"{nameof(array)} must be an array");

            if (!TypeHelper.IsIntegral(index.Type))
                throw new ArgumentException($"{nameof(index)} must be integral");

            Array = array;
            Index = index;
        }
    }

    public partial class AstNode
    {
        public static ArrayIndexNode ArrayIndexNode(AstNode array, AstNode index) => new ArrayIndexNode(array, index);
    }
}
