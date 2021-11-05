using System;
using System.Collections.Generic;
using System.Reflection;
using HeartScript.Ast.Nodes;

namespace HeartScript.Ast
{
    public class AstScope
    {
        private class Member
        {
            public AstNode Node { get; }
            public bool IsPrivate { get; }

            public Member(AstNode node, bool isPrivate)
            {
                Node = node;
                IsPrivate = isPrivate;
            }
        }
        private readonly AstScope? _parent;
        private readonly HashSet<Type> _typeWhitelist;
        private readonly Dictionary<string, Member> _members;

        public AstScope(AstScope? parent)
        {
            _parent = parent;
            _typeWhitelist = new HashSet<Type>();
            _members = new Dictionary<string, Member>(StringComparer.OrdinalIgnoreCase);
        }

        public AstScope() : this(null)
        {
        }

        public bool TryGetMember(string name, out AstNode expression)
        {
            Member? member = null;

            var current = this;
            while (current != null)
            {
                if (current._members.TryGetValue(name, out member))
                    break;

                current = current._parent;
            }

            if (member == null)
            {
                expression = null!;
                return false;
            }

            if (member.IsPrivate && current != this)
            {
                expression = null!;
                return false;
            }

            expression = member.Node;
            return true;
        }

        public void SetMember(string name, AstNode expression, bool isPrivate)
        {
            _members[name] = new Member(expression, isPrivate);
        }

        public void AllowType(Type type)
        {
            _typeWhitelist.Add(type);
        }

        public void AssertAllowed(Type type)
        {
            var current = this;
            while (current != null)
            {
                if (_typeWhitelist.Contains(type))
                    return;

                current = _parent;
            }

            throw new Exception($"{type} is not allowed");
        }
    }
}
