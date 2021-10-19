using System.Collections.Generic;
using HeartScript.Compiling;
using HeartScript.Parsing;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Expressions
{
    public class ExpressionNode : INode
    {
        public string? Name { get; }
        public string Value { get; }
        public List<INode> Children { get; }
        public int CharIndex { get; set; }

        public ExpressionNode LeftNode => (ExpressionNode)Children[_leftIndex];
        public INode MidNode => Children[_midIndex];
        public ExpressionNode RightNode => (ExpressionNode)Children[_rightIndex];

        private readonly int _leftIndex;
        private readonly int _midIndex;
        private readonly int _rightIndex;

        public ExpressionNode(string? name, ExpressionNode? leftNode, INode midNode, ExpressionNode? rightNode)
        {
            Name = name;
            Value = null;
            Children = new List<INode>();

            if (leftNode != null)
            {
                Children.Add(leftNode);
                _leftIndex = Children.Count - 1;
            }
            else
            {
                _leftIndex = -1;
            }

            Children.Add(midNode);
            _midIndex = Children.Count - 1;

            if (rightNode != null)
            {
                Children.Add(rightNode);
                _rightIndex = Children.Count - 1;
            }
            else
            {
                _rightIndex = -1;
            }

            CharIndex = Children[0].CharIndex;
        }

        public override string ToString()
        {
            return StringCompiler.Compile(this);
        }
    }
}
