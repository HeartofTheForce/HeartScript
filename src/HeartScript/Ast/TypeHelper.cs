using System;
using System.Reflection.Emit;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Ast
{
    public static class TypeHelper
    {
        public static bool CanConvert(Type a, Type b)
        {
            if (a == typeof(int) && b == typeof(double))
                return true;

            return false;
        }

        public static OpCode ConvertOpCode(Type a, Type b)
        {
            if (a != typeof(int) || b != typeof(double))
                throw new ArgumentException($"Cannot convert, {a} to {b}");

            return OpCodes.Conv_R8;
        }

        public static Type ResolveTypeNode(IParseNode typeNode)
        {
            var sequenceNode = (SequenceNode)typeNode;
            var valueNode = (ValueNode)sequenceNode.Children[0];
            var quantifierNode = (QuantifierNode)sequenceNode.Children[1];

            Type type;
            switch (valueNode.Value)
            {
                case "int": type = typeof(int); break;
                case "double": type = typeof(double); break;
                case "bool": type = typeof(bool); break;
                default: throw new NotImplementedException();
            }

            for (int i = 0; i < quantifierNode.Children.Count; i++)
            {
                type = type.MakeArrayType();
            }

            return type;
        }

        public static bool IsReal(Type type) =>
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal);

        public static bool IsIntegral(Type type) =>
            type == typeof(sbyte) ||
            type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong);
    }
}
