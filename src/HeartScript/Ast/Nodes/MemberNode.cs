using System;
using System.Reflection;

namespace HeartScript.Ast.Nodes
{
    public class MemberNode : AstNode
    {
        public AstNode? Instance { get; }
        public MemberInfo Member { get; }

        public MemberNode(AstNode? instance, MemberInfo member) : base(GetUnderlyingType(member), AstType.MemberAccess)
        {
            if (IsStatic(member) && instance != null)
                throw new ArgumentException($"{nameof(instance)} must be null for static member");

            if (!IsStatic(member) && instance == null)
                throw new ArgumentException($"{nameof(instance)} must not be null for instance member");

            Instance = instance;
            Member = member;
        }

        private static Type GetUnderlyingType(MemberInfo member)
        {
            return member switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => throw new NotImplementedException(),
            };
        }

        private static bool IsStatic(MemberInfo member)
        {
            return member switch
            {
                FieldInfo fieldInfo => fieldInfo.IsStatic,
                PropertyInfo propertyInfo => propertyInfo.GetMethod.IsStatic,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public partial class AstNode
    {
        public static MemberNode Field(AstNode? instance, FieldInfo fieldInfo) => new MemberNode(instance, fieldInfo);
        public static MemberNode Property(AstNode? instance, PropertyInfo propertyInfo) => new MemberNode(instance, propertyInfo);
    }
}
