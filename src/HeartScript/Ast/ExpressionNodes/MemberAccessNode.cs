using System;
using System.Reflection;

namespace HeartScript.Ast.Nodes
{
    public class MemberAccessNode : AstNode
    {
        public AstNode? Instance { get; }
        public MemberInfo Member { get; }

        public MemberAccessNode(AstNode? instance, MemberInfo member) : base(GetUnderlyingType(member), AstType.MemberAccess)
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
        public static MemberAccessNode Field(AstNode? instance, FieldInfo fieldInfo) => new MemberAccessNode(instance, fieldInfo);
        public static MemberAccessNode Property(AstNode? instance, PropertyInfo propertyInfo) => new MemberAccessNode(instance, propertyInfo);
    }
}
