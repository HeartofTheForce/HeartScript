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
                if (current._members.TryGetValue(name, out var candidate))
                {
                    if (candidate.IsPublic || current == this)
                    {
                        member = candidate;
                        break;
                    }
                }

                current = current._parent;
            }

            if (member != null)
            {
                expression = member.Node;
                return true;
            }

            expression = null!;
            return false;
        }

        public void SetMember(string name, AstNode expression, bool isPublic)
        {
            _members[name] = new Member(expression, isPublic);
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
                if (current._typeWhitelist.Contains(type))
                    return;

                current = _parent;
            }

            throw new Exception($"{type} is not allowed");
        }

        private class Member
        {
            public AstNode Node { get; }
            public bool IsPublic { get; }

            public Member(AstNode node, bool isPublic)
            {
                Node = node;
                IsPublic = isPublic;
            }
        }
    }
}
