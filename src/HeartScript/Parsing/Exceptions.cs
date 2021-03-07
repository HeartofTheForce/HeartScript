using System;

namespace HeartScript.Parsing
{
    public class MissingTokenException : Exception
    {
        public Token Token { get; }

        public MissingTokenException(Token token) : base($"Missing Token '{token}'")
        {
            Token = token;
        }
    }

    public class ExpressionTermException : Exception
    {
        public Token Token { get; set; }

        public ExpressionTermException(Token token) : base($"Invalid Expression Term '{token}'")
        {
            Token = token;
        }
    }

    public class UnexpectedTokenException : Exception
    {
        public Token Token { get; set; }

        public UnexpectedTokenException(Token token) : base($"Unexpected Token '{token}'")
        {
            Token = token;
        }
    }

    public class ExpressionReductionException : Exception
    {
        public int RemainingValues { get; }

        public ExpressionReductionException(int remainingValues) : base($"Expression incorrectly reduced to {remainingValues} values")
        {
            RemainingValues = remainingValues;
        }
    }
}
