using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class ExpressionNode : INode
    {
        public string Value { get; }
        public List<INode> Children { get; }

        public bool HaveLeftNode { get; }
        public bool HaveRightNodes { get; }

        public ExpressionNode(string value, INode? leftNode, IEnumerable<INode> rightNodes)
        {
            Value = value;
            Children = new List<INode>();

            HaveLeftNode = leftNode != null;
            HaveRightNodes = rightNodes.Count() > 0;

            if (HaveLeftNode || HaveRightNodes)
                Children = new List<INode>();

            if (HaveLeftNode)
                Children.Add(leftNode!);

            if (HaveRightNodes)
                Children.AddRange(rightNodes);
        }

        private IEnumerable<INode> RightNodes()
        {
            if (Children == null)
                return Enumerable.Empty<INode>();

            int offset = HaveLeftNode ? 1 : 0;

            return Children
                .Skip(offset)
                .Take(Children.Count - offset);
        }

        public static INode BuildNode(Token token, INode? leftNode, IReadOnlyList<INode> rightNodes) => new ExpressionNode(token.Value, leftNode, rightNodes);

        public override string ToString()
        {
            string? left = HaveLeftNode ? $" {Children![0]}" : null;
            string? right = RightNodes().Any() ? $" {string.Join(' ', RightNodes())}" : null;

            if (Value == "(")
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
