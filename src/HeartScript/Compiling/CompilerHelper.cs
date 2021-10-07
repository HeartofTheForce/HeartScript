using System.Collections.Generic;
using HeartScript.Expressions;
using HeartScript.Parsing;

namespace HeartScript.Compiling
{
    public static class CompilerHelper
    {
        public static List<T> GetChildren<T>(INode node)
        {
            var output = new List<T>();

            var nodeStack = new Stack<INode>();
            for (int i = node.Children.Count - 1; i >= 0; i--)
            {
                nodeStack.Push(node.Children[i]);
            }

            while (nodeStack.Count > 0)
            {
                var current = nodeStack.Pop();
                if (current is T typedNode)
                {
                    output.Add(typedNode);
                    continue;
                }

                if (current.Children != null)
                {
                    for (int i = current.Children.Count - 1; i >= 0; i--)
                    {
                        nodeStack.Push(current.Children[i]);
                    }
                }
            }

            return output;
        }
    }
}
