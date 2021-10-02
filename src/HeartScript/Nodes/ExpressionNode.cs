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

            var nodeQueue = new Queue<INode>();
            nodeQueue.Enqueue(midNode);
            while (nodeQueue.Count > 0)
            {
                var current = nodeQueue.Dequeue();
                if (current is ExpressionNode expressionNode)
                {
                    Children.Add(expressionNode);
                    continue;
                }

                if (current.Children != null)
                {
                    foreach (var child in current.Children)
                    {
                        nodeQueue.Enqueue(child);
                    }
                }
            }

            if (rightNode != null)
            {
                HaveRight = true;
                Children.Add(rightNode);
            }

            //TODO Refactor
            Value = midNode.ToString().Trim();
        }

        public override string ToString()
        {
            string? children = string.Join(' ', Children);
            if (children.Length > 0)
                children = $" {children}";

            if (Value[0] == '(')
            {
                if (!HaveLeft)
                    return children;
                else
                    return $"(${children})";
            }

            if (HaveLeft || HaveRight)
                return $"({Value}{children})";

            else
                return Value!;
        }
    }
}
