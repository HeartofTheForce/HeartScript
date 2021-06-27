using System.Collections.Generic;
using System.Linq;
using HeartScript.Nodes;
using HeartScript.Parsing;

namespace HeartScript.UTests.Helpers
{
    public class TestNode : INode
    {
        public Token Token { get; }

        public INode? LeftNode { get; }
        public IEnumerable<INode> RightNodes { get; }

        public TestNode(Token token, INode? leftNode, IEnumerable<INode> rightNodes)
        {
            Token = token;
            LeftNode = leftNode;
            RightNodes = rightNodes;
        }

        public static INode BuildNode(Token token, INode? leftNode, IReadOnlyList<INode> rightNodes) => new TestNode(token, leftNode, rightNodes);

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
