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

                if (current.Children != null)
                {
                    foreach (var child in current.Children)
                    {
                        nodeStack.Push(child);
                    }
                }
            }

            if (rightNode != null)
            {
                HaveRight = true;
                Children.Add(rightNode);
            }

            //TODO Refactor
            Value = midNode.ToString();
        }

        public ExpressionNode(string value, INode? leftNode, IEnumerable<INode> rightNodes)
        {
            Value = value;
            Children = new List<INode>();

            HaveLeft = leftNode != null;
            HaveRight = rightNodes.Count() > 0;

            if (HaveLeft || HaveRight)
                Children = new List<INode>();

            if (HaveLeft)
                Children.Add(leftNode!);

            if (HaveRight)
                Children.AddRange(rightNodes);
        }

        private IEnumerable<INode> RightNodes()
        {
            if (Children == null)
                return Enumerable.Empty<INode>();

            int offset = HaveLeft ? 1 : 0;

            return Children
                .Skip(offset)
                .Take(Children.Count - offset);
        }

        public static INode BuildNode(Token token, INode? leftNode, IReadOnlyList<INode> rightNodes) => new ExpressionNode(token.Value, leftNode, rightNodes);

        public override string ToString()
        {
            string? left = HaveLeft ? $" {Children![0]}" : null;
            string? right = RightNodes().Any() ? $" {string.Join(' ', RightNodes())}" : null;

            if (Value[0] == '(')
            {
                if (left == null)
                    return string.Join(' ', RightNodes());
                else
                    return $"(${left}{right})";
            }

            if (left != null || right != null)
                return $"({Value}{left}{right})";
            else
                return Value!;
        }
    }
}
