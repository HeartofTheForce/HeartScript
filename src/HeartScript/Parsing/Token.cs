using System;

namespace HeartScript.Parsing
{
    public class Token
    {
        public string? Value { get; }
        public int CharIndex { get; }

        public Token(
            string? value,
            int charIndex)
        {
            Value = value;
            CharIndex = charIndex;
        }

        public override string ToString()
        {
            return $"{Value ?? "EOF"} @ {CharIndex}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Token other))
                return base.Equals(obj);

            return
                other.Value == Value &&
                other.CharIndex == CharIndex;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, CharIndex);
        }
    }

    public enum Keyword
    {
        RoundOpen,
        RoundClose,
        Comma,
        Factorial,
        Plus,
        Minus,
        Multiply,
        Divide,
        BitwiseNot,
        BitwiseAnd,
        BitwiseXor,
        BitwiseOr,
        Ternary,
        Colon,
        Identifier,
        Constant,
        If,
        Else,
        EndOfString,
    }
}
