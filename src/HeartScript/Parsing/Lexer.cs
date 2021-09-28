﻿using System;

namespace HeartScript.Parsing
{
    public class Lexer
    {
        private static readonly LexerPattern s_nonSignificant = new LexerPattern("\\s*", true);

        public Token Current { get; private set; }
        public int Offset { get; private set; }
        public bool IsEOF => Offset == _input.Length;

        private readonly string _input;

        public Lexer(string input)
        {
            Current = default!;
            Offset = 0;

            _input = input;

            var nonSignificantMatch = s_nonSignificant.Regex.Match(_input, Offset);
            if (nonSignificantMatch.Success)
                Offset += nonSignificantMatch.Length;
        }

        public bool Eat(LexerPattern lexerPattern)
        {
            var match = lexerPattern.Regex.Match(_input, Offset);
            if (match.Success)
            {
                if (match.Length == 0)
                    throw new Exception($"0 length match @ {match.Index}, {lexerPattern}");

                var targetGroup = match.Groups[1];
                Current = new Token(targetGroup.Value, targetGroup.Index);
                Offset += match.Length;
            }

            var nonSignificantMatch = s_nonSignificant.Regex.Match(_input, Offset);
            if (nonSignificantMatch.Success)
                Offset += nonSignificantMatch.Length;

            return match.Success;
        }
    }
}
