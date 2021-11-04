using System;
using System.Collections.Generic;
using System.Reflection;
using HeartScript.Ast.Nodes;

namespace HeartScript.Ast
{
    public class AstScope
    {
        private readonly AstScope? _parent;
        private readonly HashSet<Type> _typeWhitelist;
        private readonly Dictionary<string, AstNode> _members;

        public AstScope(AstScope? parent)
        {
            _parent = parent;
            _typeWhitelist = new HashSet<Type>();
            _members = new Dictionary<string, AstNode>(StringComparer.OrdinalIgnoreCase);
        }

        public AstScope() : this(null)
        {
        }

        public static AstScope FromMembers(AstNode node)
        {
            var scope = new AstScope();

            var propertyInfos = node.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                scope.SetMember(propertyInfo.Name, AstNode.Property(node, propertyInfo));
            }

            var fieldInfos = node.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                scope.SetMember(fieldInfo.Name, AstNode.Field(node, fieldInfo));
            }

            scope.AllowType(node.Type);

            return scope;
        }

        public bool TryGetMember(string name, out AstNode expression)
        {
            var current = this;
            while (current != null)
            {
                if (_members.TryGetValue(name, out expression))
                    return true;

                current = _parent;
            }

            expression = null!;
            return false;
        }

        public void SetMember(string name, AstNode expression)
        {
            _members[name] = expression;
        }

        public void AllowType(Type type)
        {
            _typeWhitelist.Add(type);
        }

        public void AssertAllowed(Type type)
        {
            if (!_typeWhitelist.Contains(type))
                throw new Exception($"{type} is not allowed");
        }
    }
}
