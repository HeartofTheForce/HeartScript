using System;

namespace HeartScript.Parsing
{
    public class Token
    {
        public Keyword Keyword { get; set; }
        public string Value { get; }

        public int Line { get; }
        public int Column { get; }

        public Token(
            Keyword keyword,
            string value,
            int line,
            int column)
        {
            Keyword = keyword;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"{Keyword}, {Value}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Token other))
                return base.Equals(obj);

            return other.Keyword == Keyword && other.Value == Value;
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
        Plus,
        Minus,
        Asterisk,
        ForwardSlash,
        Identifier,
        Constant,
    }
}
