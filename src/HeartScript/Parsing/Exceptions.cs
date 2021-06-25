using System;

namespace HeartScript.Parsing
{
    public class ExpressionTermException : Exception
    {
        public Token Token { get; }

        public ExpressionTermException(Token token) : base($"Invalid Expression Term '{token}'")
        {
            Token = token;
        }
    }

    public class UnexpectedTokenException : Exception
    {
        public Token Token { get; }
        public Keyword? ExpectedKeyword { get; }

        public UnexpectedTokenException(Token token, Keyword expectedKeyword) : base($"Unexpected Token '{token}' expected '{expectedKeyword}'")
        {
            Token = token;
            ExpectedKeyword = expectedKeyword;
        }

        public UnexpectedTokenException(Token token) : base($"Unexpected Token '{token}'")
        {
            Token = token;
        }
    }
}
