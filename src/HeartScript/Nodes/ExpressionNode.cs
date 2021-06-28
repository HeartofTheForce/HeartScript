using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class ExpressionNode : INode
    {
        public Token Token { get; }

        public INode? LeftNode { get; }
        public IEnumerable<INode> RightNodes { get; }

        public ExpressionNode(Token token, INode? leftNode, IEnumerable<INode> rightNodes)
        {
            Token = token;
            LeftNode = leftNode;
            RightNodes = rightNodes;
        }

        public static INode BuildNode(Token token, INode? leftNode, IReadOnlyList<INode> rightNodes) => new ExpressionNode(token, leftNode, rightNodes);

        public override string ToString()
        {
            string? left = LeftNode != null ? $" {LeftNode}" : null;
            string? right = RightNodes.Any() ? $" {string.Join(' ', RightNodes)}" : null;

            if (Token.Value == "(")
            {
                if (left == null)
                    return string.Join(' ', RightNodes);
                else
                    return $"(${left}{right})";
            }

            if (left != null || right != null)
                return $"({Token.Value}{left}{right})";
            else
                return Token.Value!;
        }
    }
}
