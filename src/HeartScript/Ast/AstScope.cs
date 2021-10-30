using System;
using System.Collections.Generic;
using System.Reflection;
using HeartScript.Ast.Nodes;

namespace HeartScript.Ast
{
    public class AstScope
    {
        private readonly Dictionary<string, AstNode> _variables;

        private AstScope(Dictionary<string, AstNode> variables)
        {
            _variables = variables;
        }

        public static AstScope Empty()
        {
            var variables = new Dictionary<string, AstNode>();
            return new AstScope(variables);
        }

        public static AstScope FromMembers(AstNode node)
        {
            var variables = new Dictionary<string, AstNode>(StringComparer.OrdinalIgnoreCase);

            var propertyInfos = node.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                variables[propertyInfo.Name] = AstNode.Property(node, propertyInfo);
            }

            var fieldInfos = node.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                variables[fieldInfo.Name] = AstNode.Field(node, fieldInfo);
            }

            return new AstScope(variables);
        }

        public bool TryGetVariable(string name, out AstNode expression)
        {
            return _variables.TryGetValue(name, out expression);
        }
    }
}
