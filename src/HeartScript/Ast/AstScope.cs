using System;
using System.Collections.Generic;
using System.Reflection;
using HeartScript.Ast.Nodes;

namespace HeartScript.Ast
{
    public class AstScope
    {
        private readonly HashSet<Type> _typeWhitelist;
        private readonly Dictionary<string, AstNode> _variables;

        private AstScope(Dictionary<string, AstNode> variables)
        {
            _typeWhitelist = new HashSet<Type>()
            {
                typeof(int),
                typeof(double),
                typeof(bool),
                typeof(Math),
            };

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

            var scope = new AstScope(variables);
            scope._typeWhitelist.Add(node.Type);

            return scope;
        }

        public bool TryGetVariable(string name, out AstNode expression)
        {
            return _variables.TryGetValue(name, out expression);
        }

        public void AssertAllowed(Type type)
        {
            if (!_typeWhitelist.Contains(type))
                throw new Exception($"{type} is not allowed");
        }
    }
}
