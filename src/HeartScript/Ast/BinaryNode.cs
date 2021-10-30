using System;

namespace HeartScript.Ast
{
    public class BinaryNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }

        public BinaryNode(AstNode left, AstNode right) : base(left.Type)
        {
            if (left.Type != right.Type)
                throw new ArgumentException($"{nameof(left)} and {nameof(right)} must be the same Type");

            Left = left;
            Right = right;
        }
    }
}
