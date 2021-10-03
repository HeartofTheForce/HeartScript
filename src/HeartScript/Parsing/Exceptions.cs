using System;

namespace HeartScript.Parsing
{
    public class ExpressionTermException : PatternException
    {
        public ExpressionTermException(int charIndex) : base(charIndex, $"Invalid Expression Term @ {charIndex}")
        {
        }
    }

    public class UnexpectedTokenException : PatternException
    {
        public string ExpectedPattern { get; }

        public UnexpectedTokenException(int charIndex, string expectedPattern) : base(charIndex, $"Unexpected Token @ {charIndex} expected {expectedPattern}")
        {
            ExpectedPattern = expectedPattern;
        }

        public UnexpectedTokenException(int charIndex, LexerPattern lexerPattern) : this(charIndex, lexerPattern.ToString())
        {
        }
    }
}
