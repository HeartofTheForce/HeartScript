using System;

namespace HeartScript.Parsing
{
    public class Token
    {
        public Keyword Keyword { get; set; }
        public string Value { get; }
        public int CharOffset { get; }

        public Token(
            Keyword keyword,
            string value,
            int charOffset)
        {
            Keyword = keyword;
            Value = value;
            CharOffset = charOffset;
        }

        public override string ToString()
        {
            return $"({Keyword} '{Value}') @ {CharOffset}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Token other))
                return base.Equals(obj);

            return
                other.Keyword == Keyword &&
                other.Value == Value &&
                other.CharOffset == CharOffset;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Keyword, Value);
        }
    }

    public enum Keyword
    {
        Space,
        Tab,
        Newline,
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
        StartOfString,
        EndOfString,
    }
}
