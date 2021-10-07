using System.Collections.Generic;
using HeartScript.Compiling;
using HeartScript.Parsing;
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace HeartScript.Expressions
{
    public class ExpressionNode : INode
    {
        public string? Name { get; set; }
        public string Value { get; }
        public List<INode> Children { get; }
        public int CharIndex { get; set; }

        public ExpressionNode LeftNode => (ExpressionNode)Children[LeftIndex];
        public INode MidNode => Children[MidIndex];
        public ExpressionNode RightNode => (ExpressionNode)Children[RightIndex];

        public int LeftIndex { get; }
        public int MidIndex { get; }
        public int RightIndex { get; }

        public ExpressionNode(ExpressionNode? leftNode, INode midNode, ExpressionNode? rightNode)
        {
            Name = midNode.Name;
            Value = null;
            Children = new List<INode>();

            if (leftNode != null)
            {
                Children.Add(leftNode);
                LeftIndex = Children.Count - 1;
            }
            else
            {
                LeftIndex = -1;
            }

            Children.Add(midNode);
            MidIndex = Children.Count - 1;

            if (rightNode != null)
            {
                Children.Add(rightNode);
                RightIndex = Children.Count - 1;
            }
            else
            {
                RightIndex = -1;
            }

            CharIndex = Children[0].CharIndex;
        }

        public override string ToString()
        {
            return StringCompiler.Compile(this);
        }
    }
}
