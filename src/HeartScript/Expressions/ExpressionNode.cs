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

        public bool HaveLeft { get; }
        public bool HaveRight { get; }

        public ExpressionNode(INode? leftNode, INode midNode, INode? rightNode)
        {
            Name = midNode.Name;
            Value = null;
            Children = new List<INode>();

            if (leftNode != null)
            {
                HaveLeft = true;
                Children.Add(leftNode);
            }

            var nodeStack = new Stack<INode>();
            nodeStack.Push(midNode);
            while (nodeStack.Count > 0)
            {
                var current = nodeStack.Pop();
                if (current is ExpressionNode expressionNode)
                {
                    Children.Add(expressionNode);
                    continue;
                }

                if (current.Value != null)
                {
                    if (Value == null)
                        Value = current.Value;

                    continue;
                }

                for (int i = current.Children.Count - 1; i >= 0; i--)
                {
                    nodeStack.Push(current.Children[i]);
                }
            }

            if (rightNode != null)
            {
                HaveRight = true;
                Children.Add(rightNode);
            }

            if (Children.Count > 0)
                CharIndex = Children[0].CharIndex;
            else
                CharIndex = midNode.CharIndex;
        }

        public override string ToString()
        {
            return StringCompiler.Compile(this);
        }
    }
}
