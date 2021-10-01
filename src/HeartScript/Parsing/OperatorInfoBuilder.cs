using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Nodes;
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable IDE0018

namespace HeartScript.Parsing
{
    public static class OperatorInfoBuilder
    {
        private static readonly LexerPattern s_regex = LexerPattern.FromRegex("`(?:``|[^`])*`");
        private static readonly LexerPattern s_plainText = LexerPattern.FromRegex("'(?:''|[^'])*'");
        private static readonly LexerPattern s_digits = LexerPattern.FromRegex("\\d+");
        private static readonly LexerPattern s_none = LexerPattern.FromPlainText("none");
        private static readonly LexerPattern s_any = LexerPattern.FromPlainText("any");

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
            Token current;

            uint? leftPrecedence;
            if (lexer.TryEat(s_digits, out current))
                leftPrecedence = uint.Parse(current.Value);
            else if (lexer.TryEat(s_none, out current))
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
            if (lexer.TryEat(s_digits, out current))
                rightPrecedence = uint.Parse(current.Value);
            else
                throw new Exception($"{nameof(rightPrecedence)} @ {lexer.Offset}");

            uint? rightOperands;
            if (lexer.TryEat(s_digits, out current))
                rightOperands = uint.Parse(current.Value);
            else if (lexer.TryEat(s_any, out current))
                rightOperands = null;
            else
                throw new OperatorInfoBuilderException(nameof(rightOperands), lineNumber, lexer.Offset);

            LexerPattern? delimiter;
            if (TryEatRegex(lexer, out var rDelimiter))
                delimiter = rDelimiter;
            else if (TryEatPlainText(lexer, out var ptDelimiter))
                delimiter = ptDelimiter;
            else if (lexer.TryEat(s_any, out current))
                delimiter = null;
            else
                throw new OperatorInfoBuilderException(nameof(delimiter), lineNumber, lexer.Offset);

            LexerPattern? terminator;
            if (TryEatRegex(lexer, out var rTerminator))
                terminator = rTerminator;
            else if (TryEatPlainText(lexer, out var ptTerminator))
                terminator = ptTerminator;
            else if (lexer.TryEat(s_any, out current))
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
            bool success = lexer.TryEat(s_regex, out var current);

            if (success)
            {
                string? pattern = current.Value[1..^1].Replace("``", "`");
                lexerPattern = LexerPattern.FromRegex(pattern);
            }
            else
                lexerPattern = null;

            return success;
        }

        private static bool TryEatPlainText(Lexer lexer, out LexerPattern? lexerPattern)
        {
            bool success = lexer.TryEat(s_plainText, out var current);

            if (success)
            {
                string? pattern = current.Value[1..^1].Replace("''", "'");
                lexerPattern = LexerPattern.FromPlainText(pattern);
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
