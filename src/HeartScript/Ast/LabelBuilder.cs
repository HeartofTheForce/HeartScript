using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using HeartScript.Peg.Patterns;
#pragma warning disable IDE0066

namespace HeartScript.Ast
{
    public static class LabelBuilder
    {
        private delegate AstNode AstNodeBuilder(AstScope scope, LabelNode node);

        private static readonly Dictionary<string, AstNodeBuilder> s_nodeBuilders = new Dictionary<string, AstNodeBuilder>()
        {
            ["method"] = BuildMethod,
        };

        public static AstNode Build(AstScope scope, LabelNode node)
        {
            if (node.Label != null && s_nodeBuilders.TryGetValue(node.Label, out var builder))
                return builder(scope, node);

            throw new ArgumentException($"{node.Label} does not have a matching builder");
        }

        static AstNode BuildMethod(AstScope scope, LabelNode node)
        {
            var sequenceNode = (SequenceNode)node.Node;
            var expressionNode = sequenceNode.Children[6];

            return AstBuilder.Build(scope, expressionNode);
        }
    }
}
