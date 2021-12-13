using System;
using System.Collections.Generic;

namespace HeartScript.Parsing
{
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

            if (Exception == null || ex.TextOffset >= Exception.TextOffset && ex.Priority >= Exception.Priority)
                Exception = ex;
        }

        public void AssertComplete()
        {
            if (Offset != Input.Length)
            {
                LogException(new UnexpectedTokenException(Offset, "EOF"));
                if (Exception != null)
                    throw Exception;
                else
                    throw new ArgumentException(nameof(Exception));
            }
        }
    }

    public abstract class PatternException : Exception
    {
        public int TextOffset { get; }
        public int Priority { get; }

        public PatternException(int textOffset, int priority, string message) : base(message)
        {
            TextOffset = textOffset;
            Priority = priority;
        }

        public PatternException(int textOffset, string message) : this(textOffset, 0, message)
        {
        }
    }

    public class ZeroLengthMatchException : PatternException
    {
        public ZeroLengthMatchException(int textOffset) : base(textOffset, "Unexpected 0 length match")
        {
        }
    }
}
