using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Nodes;
#pragma warning disable CS8604

namespace HeartScript.Parsing
{
    public static class OperatorInfoBuilder
    {
        private static readonly LexerPattern s_regex = new LexerPattern("`((?:``|[^`])*)`", true);
        private static readonly LexerPattern s_plainText = new LexerPattern("'((?:''|[^'])*)'", true);
        private static readonly LexerPattern s_digits = new LexerPattern("\\d+", true);
        private static readonly LexerPattern s_none = new LexerPattern("none", true);
        private static readonly LexerPattern s_any = new LexerPattern("any", true);

        public static IEnumerable<OperatorInfo> Parse(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            var output = new List<OperatorInfo>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var operatorInfo = ParseOperatorInfo(line, i);
                output.Add(operatorInfo);
            }

            return output;
        }

        private static OperatorInfo ParseOperatorInfo(string input, int lineNumber)
        {
            var lexer = new Lexer(input);

            uint? leftPrecedence;
            if (lexer.Eat(s_digits))
                leftPrecedence = uint.Parse(lexer.Current.Value);
            else if (lexer.Eat(s_none))
                leftPrecedence = null;
            else
                throw new OperatorInfoBuilderException(nameof(leftPrecedence), lineNumber, lexer.Offset);

            LexerPattern keyword;
            if (lexer.Eat(s_regex))
                keyword = new LexerPattern(lexer.Current.Value, true);
            else if (lexer.Eat(s_plainText))
                keyword = new LexerPattern(lexer.Current.Value, false);
            else
                throw new OperatorInfoBuilderException(nameof(keyword), lineNumber, lexer.Offset);

            uint rightPrecedence;
            if (lexer.Eat(s_digits))
                rightPrecedence = uint.Parse(lexer.Current.Value);
            else
                throw new Exception($"{nameof(rightPrecedence)} @ {lexer.Offset}");

            uint? rightOperands;
            if (lexer.Eat(s_digits))
                rightOperands = uint.Parse(lexer.Current.Value);
            else if (lexer.Eat(s_any))
                rightOperands = null;
            else
                throw new OperatorInfoBuilderException(nameof(rightOperands), lineNumber, lexer.Offset);

            LexerPattern? delimiter = null;
            if (lexer.Eat(s_regex))
                delimiter = new LexerPattern(lexer.Current.Value, true);
            else if (lexer.Eat(s_plainText))
                delimiter = new LexerPattern(lexer.Current.Value, false);
            else if (lexer.Eat(s_any))
                delimiter = null;
            else
                throw new OperatorInfoBuilderException(nameof(delimiter), lineNumber, lexer.Offset);

            LexerPattern? terminator;
            if (lexer.Eat(s_regex))
                terminator = new LexerPattern(lexer.Current.Value, true);
            else if (lexer.Eat(s_plainText))
                terminator = new LexerPattern(lexer.Current.Value, false);
            else if (lexer.Eat(s_any))
                terminator = null;
            else
                throw new OperatorInfoBuilderException(nameof(terminator), lineNumber, lexer.Offset);

            return new OperatorInfo(
                keyword,
                leftPrecedence,
                rightPrecedence,
                rightOperands,
                delimiter,
                terminator,
                ExpressionNode.BuildNode);
        }

        private class OperatorInfoBuilderException : Exception
        {
            public OperatorInfoBuilderException(string fieldName, int lineNumber, int charIndex) : base($"{fieldName} expected @ {lineNumber}, {charIndex}")
            {
            }
        }
    }
}
