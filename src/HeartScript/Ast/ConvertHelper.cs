using System;
using System.Reflection.Emit;

namespace HeartScript.Ast
{
    public static class ConvertHelper
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
