using System;
using System.Reflection;

namespace HeartScript.Ast.Nodes
{
    public class MemberNode : AstNode
    {
        public AstNode? Expression { get; }
        public MemberInfo Member { get; }

        public MemberNode(AstNode? expression, MemberInfo member) : base(GetUnderlyingType(member), AstType.MemberAccess)
        {
            if (IsStatic(member) && expression != null)
                throw new ArgumentException($"{nameof(expression)} must be null for static member");

            if (!IsStatic(member) && expression == null)
                throw new ArgumentException($"{nameof(expression)} must not be null for instance member");

            Expression = expression;
            Member = member;
        }

        private static Type GetUnderlyingType(MemberInfo member)
        {
            return member.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Method => ((MethodInfo)member).ReturnType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                _ => throw new NotImplementedException(),
            };
        }

        private static bool IsStatic(MemberInfo member)
        {
            return member.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)member).IsStatic,
                MemberTypes.Method => ((MethodInfo)member).IsStatic,
                MemberTypes.Property => ((PropertyInfo)member).GetMethod.IsStatic,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public partial class AstNode
    {
        public static ConstantNode MemberAccess(string name) => new ConstantNode("TEMP_MEMBER_ACCESS");
    }
}
