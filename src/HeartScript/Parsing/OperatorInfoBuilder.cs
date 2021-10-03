using System;
using System.Collections.Generic;
using System.IO;
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable IDE0018

namespace HeartScript.Parsing
{
    public static class OperatorInfoBuilder
    {
        private static readonly LexerPattern s_digits = LexerPattern.FromRegex("\\d+");
        private static readonly LexerPattern s_none = LexerPattern.FromPlainText("none");

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
            var parser = PegBuilder.CreateParser();
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
            var builderCtx = PegBuilder.CreateBuilder();
            var operatorInfo = builderCtx.BuildKeyPattern(patternNode);

            return new OperatorInfo(operatorInfo, leftPrecedence, rightPrecedence);
        }
    }
}
