using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Expressions;
using HeartScript.Parsing;

namespace HeartScript.Compiling
{
    public static class StringCompiler
    {
        private static readonly Dictionary<string, Func<INode, string>> s_overrideCompilers = new Dictionary<string, Func<INode, string>>()
        {
            ["()"] = (node) => node.Children[0].Children[1].ToString(),
            ["real"] = (node) => node.Children[0].Value,
            ["integral"] = (node) => node.Children[0].Value,
            ["boolean"] = (node) => node.Children[0].Children[0].Value,
            ["identifier"] = (node) => node.Children[0].Value,
        };

        public static string Compile(INode node)
        {
            if (node.Name != null)
            {
                if (s_overrideCompilers.TryGetValue(node.Name, out var compiler))
                    return compiler(node);

                return CompileString(node.Name, node);
            }

            throw new ArgumentException($"{nameof(node.Name)} cannot be null");
        }

        private static string CompileString(string operatorSymbol, INode node)
        {
            IEnumerable<INode> children;
            if (node is ExpressionNode expressionNode)
                children = CompilerHelper.GetChildren<ExpressionNode>(expressionNode);
            else
                children = node.Children;

            if (children.Count() > 0)
            {
                string parameters = string.Join(' ', children);
                return $"({operatorSymbol} {parameters})";
            }

            return $"({operatorSymbol})";
        }
    }
}
