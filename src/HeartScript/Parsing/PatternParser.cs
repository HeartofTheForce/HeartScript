using System;
using System.Collections.Generic;

namespace HeartScript.Parsing
{
    public class PatternParser
    {
        public Dictionary<string, IPattern> Patterns { get; }

        public PatternParser()
        {
            Patterns = new Dictionary<string, IPattern>();
        }
    }

    public class ParserContext
    {
        public string Input { get; }
        public int Offset { get; set; }
        public PatternException? Exception { get; private set; }
        public bool IsEOF => Offset == Input.Length;

        private readonly List<PatternException> _patternExceptions;

        public ParserContext(string input)
        {
            Input = input;
            Offset = 0;
            Exception = null;
            _patternExceptions = new List<PatternException>();
        }

        public void LogException(PatternException ex)
        {
            _patternExceptions.Add(ex);

            if (Exception == null || Exception.CharIndex <= Offset)
                Exception = ex;
        }
    }

    public abstract class PatternException : Exception
    {
        public int CharIndex { get; }

        public PatternException(int charIndex, string message) : base(message)
        {
            CharIndex = charIndex;
        }
    }

    public class ZeroLengthMatchException : PatternException
    {
        public ZeroLengthMatchException(int charIndex) : base(charIndex, "Unexpected 0 length match")
        {
        }
    }
}
