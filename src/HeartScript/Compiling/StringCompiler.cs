using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Expressions;
using HeartScript.Parsing;
using HeartScript.Peg.Patterns;

namespace HeartScript.Compiling
{
    public static class StringCompiler
    {
        private static readonly Dictionary<string, Func<ExpressionNode, string>> s_overrideCompilers = new Dictionary<string, Func<ExpressionNode, string>>()
        {
            ["()"] = (node) =>
            {
                var sequenceNode = (SequenceNode)node.MidNode;
                var lookupNode = (LookupNode)sequenceNode.Children[1];
                return Compile(lookupNode.Node);
            },
            ["real"] = (node) =>
            {
                var valueNode = (ValueNode)node.MidNode;
                return valueNode.Value;
            },
            ["integral"] = (node) =>
            {
                var valueNode = (ValueNode)node.MidNode;
                return valueNode.Value;
            },
            ["boolean"] = (node) =>
            {
                var choiceNode = (ChoiceNode)node.MidNode;
                var valueNode = (ValueNode)choiceNode.Node;
                return valueNode.Value;
            },
            ["identifier"] = (node) =>
            {
                var valueNode = (ValueNode)node.MidNode;
                return valueNode.Value;
            },
        };

        public static string Compile(IParseNode node)
        {
            if (node is ExpressionNode expressionNode)
            {
                if (s_overrideCompilers.TryGetValue(expressionNode.Key, out var compiler))
                    return compiler(expressionNode);

                IEnumerable<IParseNode> children = ParseNodeHelper.GetChildren<ExpressionNode>(expressionNode);

                if (children.Count() > 0)
                {
                    string parameters = string.Join(' ', children);
                    return $"({expressionNode.Key} {parameters})";
                }

                return $"({expressionNode.Key})";
            }

            throw new NotImplementedException();
        }
    }
}
