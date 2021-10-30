using System;

namespace HeartScript.Ast.Nodes
{
    public class BinaryNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }

        public BinaryNode(AstNode left, AstNode right, AstType nodeType) : base(left.Type, nodeType)
        {
            if (left.Type != right.Type)
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} must be the same Type");

            Left = left;
            Right = right;
        }
    }

    public partial class AstNode
    {
        public static BinaryNode Multiply(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.Multiply);
        public static BinaryNode Divide(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.Divide);
        public static BinaryNode Add(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.Add);
        public static BinaryNode Subtract(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.Subtract);
        public static BinaryNode LessThanOrEqual(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.LessThanOrEqual);
        public static BinaryNode GreaterThanOrEqual(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.GreaterThanOrEqual);
        public static BinaryNode LessThan(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.LessThan);
        public static BinaryNode GreaterThan(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.GreaterThan);
        public static BinaryNode Equal(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.Equal);
        public static BinaryNode NotEqual(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.NotEqual);
        public static BinaryNode And(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.And);
        public static BinaryNode ExclusiveOr(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.ExclusiveOr);
        public static BinaryNode Or(AstNode left, AstNode right) => new BinaryNode(left, right, AstType.Or);
    }
}
