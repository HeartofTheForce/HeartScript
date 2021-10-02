using System;
using System.Collections.Generic;
using System.IO;
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable IDE0018

namespace HeartScript.Parsing
{
    public static class OperatorPatternBuilder
    {
        private static readonly LexerPattern s_digits = LexerPattern.FromRegex("\\d+");
        private static readonly LexerPattern s_none = LexerPattern.FromPlainText("none");

        public static IEnumerable<OperatorPattern> Parse(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            var output = new List<OperatorPattern>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var operatorPattern = ParseOperatorPattern(line, i);
                output.Add(operatorPattern);
            }

            return output;
        }

        private static OperatorPattern ParseOperatorPattern(string input, int lineNumber)
        {
            var parser = OperatorPatternPegBuilder.CreateParser();
            var ctx = new ParserContext(input);

            var pattern = SequencePattern.Create()
                .Then(ChoicePattern.Create()
                    .Or(s_digits)
                    .Or(s_none))
                .Then(LexerPattern.FromPlainText(" "))
                .Then(ChoicePattern.Create()
                    .Or(s_digits)
                    .Or(s_none))
                .Then(LexerPattern.FromPlainText(" "))
                .Then(KeyPattern.Create("choice"));

            var result = parser.TryMatch(pattern, ctx);

            if (result.ErrorMessage != null)
                throw new Exception($"{result.ErrorMessage}, {lineNumber}");

            var leftNode = (ChoiceNode)result.Value.Children[0];
            uint? leftPrecedence;
            if (uint.TryParse(leftNode.ChoiceValue.Value, out uint leftValue))
                leftPrecedence = leftValue;
            else
                leftPrecedence = null;

            var rightNode = (ChoiceNode)result.Value.Children[2];
            uint? rightPrecedence;
            if (uint.TryParse(rightNode.ChoiceValue.Value, out uint rightValue))
                rightPrecedence = rightValue;
            else
                rightPrecedence = null;

            var patternNode = (KeyNode)result.Value.Children[4];
            var builderCtx = OperatorPatternPegBuilder.CreateBuilder();
            var operatorPattern = builderCtx.BuildKeyPattern(patternNode);

            return new OperatorPattern(operatorPattern, leftPrecedence, rightPrecedence);
        }
    }
}
