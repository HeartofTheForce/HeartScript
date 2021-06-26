using System;

namespace HeartScript.Parsing
{
    public class Token
    {
        public string Value { get; }
        public int CharOffset { get; }

        public Token(
            string value,
            int charOffset)
        {
            Value = value;
            CharOffset = charOffset;
        }

        public override string ToString()
        {
            return $"{Value} @ {CharOffset}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Token other))
                return base.Equals(obj);

            return
                other.Value == Value &&
                other.CharOffset == CharOffset;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, CharOffset);
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
