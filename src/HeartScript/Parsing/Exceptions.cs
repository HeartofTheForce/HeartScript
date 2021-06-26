using System;

namespace HeartScript.Parsing
{
    public class ExpressionTermException : Exception
    {
        public int CharIndex { get; }

        public ExpressionTermException(int charIndex) : base($"Invalid Expression Term @ {charIndex}")
        {
            CharIndex = charIndex;
        }
    }

    public class UnexpectedTokenException : Exception
    {
        public int CharIndex { get; }
        public string ExpectedPattern { get; }

        public UnexpectedTokenException(int charIndex, string expectedPattern) : base($"Unexpected Token @ {charIndex} expected {expectedPattern}")
        {
            CharIndex = charIndex;
            ExpectedPattern = expectedPattern;
        }

        public UnexpectedTokenException(int charIndex, LexerPattern lexerPattern) : this(charIndex, lexerPattern.ToString())
        {
        }
    }
}
