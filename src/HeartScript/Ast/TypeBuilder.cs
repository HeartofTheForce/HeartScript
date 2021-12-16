using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using Heart.Parsing.Patterns;
using Heart.Parsing;

namespace HeartScript.Ast
{
    public static class TypeBuilder
    {
        private delegate AstNode TypeNodeBuilder(SymbolScope scope, IParseNode node);

        private static readonly Dictionary<string, TypeNodeBuilder> s_nodeBuilders = new Dictionary<string, TypeNodeBuilder>()
        {
            ["method"] = MethodBuilder.BuildMethod,
        };

        public static AstNode Build(SymbolScope scope, LabelNode node)
        {
            if (node.Label != null && s_nodeBuilders.TryGetValue(node.Label, out var builder))
                return builder(scope, node.Node);

            throw new ArgumentException($"{node.Label} has no matching builder");
        }
    }
}
