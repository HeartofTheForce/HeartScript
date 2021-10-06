using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Compiling
{
    public static class StringCompiler
    {
        private static readonly Dictionary<string, Func<INode, string>> s_overrideCompilers = new Dictionary<string, Func<INode, string>>()
        {
            ["()"] = (node) => node.Children[0].ToString(),
            ["Constant"] = (node) => node.Value,
            ["Identifier"] = (node) => node.Value,
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
            if (node.Children.Count > 0)
            {
                string? children = string.Join(' ', node.Children);
                return $"({operatorSymbol} {children})";
            }

            return $"({operatorSymbol})";
        }
    }
}
