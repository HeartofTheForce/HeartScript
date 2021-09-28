using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Nodes;
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604

namespace HeartScript.Parsing
{
    public static class OperatorInfoBuilder
    {
        private static readonly LexerPattern s_regex = new LexerPattern("`(?:``|[^`])*`", true);
        private static readonly LexerPattern s_plainText = new LexerPattern("'(?:''|[^'])*'", true);
        private static readonly LexerPattern s_digits = new LexerPattern("\\d+", true);
        private static readonly LexerPattern s_none = new LexerPattern("none", false);
        private static readonly LexerPattern s_any = new LexerPattern("any", false);

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
            if (TryEatRegex(lexer, out var rKeyword))
                keyword = rKeyword;
            else if (TryEatPlainText(lexer, out var ptKeyword))
                keyword = ptKeyword;
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

            LexerPattern? delimiter;
            if (TryEatRegex(lexer, out var rDelimiter))
                delimiter = rDelimiter;
            else if (TryEatPlainText(lexer, out var ptDelimiter))
                delimiter = ptDelimiter;
            else if (lexer.Eat(s_any))
                delimiter = null;
            else
                throw new OperatorInfoBuilderException(nameof(delimiter), lineNumber, lexer.Offset);

            LexerPattern? terminator;
            if (TryEatRegex(lexer, out var rTerminator))
                terminator = rTerminator;
            else if (TryEatPlainText(lexer, out var ptTerminator))
                terminator = ptTerminator;
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

        private static bool TryEatRegex(Lexer lexer, out LexerPattern? lexerPattern)
        {
            bool success = lexer.Eat(s_regex);

            if (success)
            {
                string? pattern = lexer.Current.Value[1..^1].Replace("``", "`");
                lexerPattern = new LexerPattern(pattern, true);
            }
            else
                lexerPattern = null;

            return success;
        }

        private static bool TryEatPlainText(Lexer lexer, out LexerPattern? lexerPattern)
        {
            bool success = lexer.Eat(s_plainText);

            if (success)
            {
                string? pattern = lexer.Current.Value[1..^1].Replace("''", "'");
                lexerPattern = new LexerPattern(pattern, false);
            }
            else
                lexerPattern = null;

            return success;
        }

        private class OperatorInfoBuilderException : Exception
        {
            public OperatorInfoBuilderException(string fieldName, int lineNumber, int charIndex) : base($"{fieldName} expected @ {lineNumber}, {charIndex}")
            {
            }
        }
    }
}
