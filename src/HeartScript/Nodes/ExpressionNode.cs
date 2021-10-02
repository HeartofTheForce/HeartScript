using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class ExpressionNode : INode
    {
        public string Value { get; }
        public List<INode> Children { get; }

        public bool HaveLeft { get; }
        public bool HaveRight { get; }

        public ExpressionNode(string value)
        {
            Value = value;
            Children = null;
            HaveLeft = false;
            HaveRight = false;
        }

        public ExpressionNode(INode? leftNode, INode midNode, INode? rightNode)
        {
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
                else if (current.Value != null)
                {
                    //TODO Refactor
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
        }

        public override string ToString()
        {
            string? children = string.Join(' ', Children);

            string op = Value;
            if (op == "(")
            {
                if (!HaveLeft)
                    return children;
                else
                    op = "$";
            }

            if (children.Length > 0)
                return $"({op} {children})";
            else
                return Value!;
        }
    }
}
