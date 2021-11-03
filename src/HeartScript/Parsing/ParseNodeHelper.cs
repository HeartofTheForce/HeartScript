using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Expressions;
using HeartScript.Peg.Patterns;

namespace HeartScript.Parsing
{
    public static class ParseNodeHelper
    {
        public static List<T> GetChildren<T>(IParseNode node)
        {
            var output = new List<T>();

            var nodeStack = new Stack<IParseNode>();
            foreach (var child in GetChildren(node).Reverse())
            {
                nodeStack.Push(child);
            }

            while (nodeStack.Count > 0)
            {
                var current = nodeStack.Pop();
                if (current is T typedNode)
                {
                    output.Add(typedNode);
                    continue;
                }

                foreach (var child in GetChildren(current).Reverse())
                {
                    nodeStack.Push(child);
                }
            }

            return output;
        }

        private static IEnumerable<IParseNode> GetChildren(IParseNode node)
        {
            switch (node)
            {
                case ValueNode _:
                    return Enumerable.Empty<IParseNode>();
                case ChoiceNode choiceNode:
                    return Enumerable.Repeat(choiceNode.Node, 1);
                case SequenceNode sequenceNode:
                    return sequenceNode.Children;
                case QuantifierNode quantifierNode:
                    return quantifierNode.Children;
                case ExpressionNode expressionNode:
                    {
                        var output = new List<IParseNode>();

                        if (expressionNode.LeftNode != null)
                            output.Add(expressionNode.LeftNode);

                        output.Add(expressionNode.MidNode);

                        if (expressionNode.RightNode != null)
                            output.Add(expressionNode.RightNode);

                        return output;
                    }
                default: throw new NotImplementedException();
            }
        }
    }
}
