using System;

namespace HeartScript.Ast.Nodes
{
    public class BinaryNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }

        public BinaryNode(AstNode left, AstNode right, Type type, AstType nodeType) : base(type, nodeType)
        {
            if (left.Type != right.Type)
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} must be the same Type");

            Left = left;
            Right = right;
        }
    }

    public partial class AstNode
    {
        public static BinaryNode Multiply(AstNode left, AstNode right) => new BinaryNode(left, right, left.Type, AstType.Multiply);
        public static BinaryNode Divide(AstNode left, AstNode right) => new BinaryNode(left, right, left.Type, AstType.Divide);
        public static BinaryNode Add(AstNode left, AstNode right) => new BinaryNode(left, right, left.Type, AstType.Add);
        public static BinaryNode Subtract(AstNode left, AstNode right) => new BinaryNode(left, right, left.Type, AstType.Subtract);
        public static BinaryNode LessThanOrEqual(AstNode left, AstNode right) => new BinaryNode(left, right, typeof(bool), AstType.LessThanOrEqual);
        public static BinaryNode GreaterThanOrEqual(AstNode left, AstNode right) => new BinaryNode(left, right, typeof(bool), AstType.GreaterThanOrEqual);
        public static BinaryNode LessThan(AstNode left, AstNode right) => new BinaryNode(left, right, typeof(bool), AstType.LessThan);
        public static BinaryNode GreaterThan(AstNode left, AstNode right) => new BinaryNode(left, right, typeof(bool), AstType.GreaterThan);
        public static BinaryNode Equal(AstNode left, AstNode right) => new BinaryNode(left, right, typeof(bool), AstType.Equal);
        public static BinaryNode NotEqual(AstNode left, AstNode right) => new BinaryNode(left, right, typeof(bool), AstType.NotEqual);
        public static BinaryNode And(AstNode left, AstNode right) => new BinaryNode(left, right, left.Type, AstType.And);
        public static BinaryNode ExclusiveOr(AstNode left, AstNode right) => new BinaryNode(left, right, left.Type, AstType.ExclusiveOr);
        public static BinaryNode Or(AstNode left, AstNode right) => new BinaryNode(left, right, left.Type, AstType.Or);
    }
}
