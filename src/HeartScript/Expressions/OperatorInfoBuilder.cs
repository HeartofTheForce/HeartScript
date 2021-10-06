using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Parsing;
using HeartScript.Peg;
using HeartScript.Peg.Nodes;
using HeartScript.Peg.Patterns;

namespace HeartScript.Expressions
{
    public static class OperatorInfoBuilder
    {
        private static readonly LexerPattern s_name = LexerPattern.FromRegex("'[^']*'");
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
            var ctx = new ParserContext(input);
            var pegParser = PegHelper.CreatePegParser();
            var pattern = SequencePattern.Create()
                .Then(QuantifierPattern.Optional(SequencePattern.Create()
                    .Then(s_name)
                    .Discard(LexerPattern.FromRegex("\\s+"))
                    .Discard(LexerPattern.FromRegex("->"))))
                .Discard(LexerPattern.FromRegex("\\s+"))
                .Then(ChoicePattern.Create()
                    .Or(s_digits)
                    .Or(s_none))
                .Discard(LexerPattern.FromRegex("\\s+"))
                .Then(ChoicePattern.Create()
                    .Or(s_digits)
                    .Or(s_none))
                .Discard(LexerPattern.FromRegex("\\s+"))
                .Then(LookupPattern.Create("peg"));

            var result = pegParser.TryMatch(pattern, ctx);

            if (result == null)
                throw new Exception($"{ctx.Exception}, {lineNumber}");

            string? name = null;
            var optionalNode = result.Children[0];
            if (optionalNode.Children.Count > 0)
            {
                var nameNode = optionalNode.Children[0];
                name = nameNode.Value[1..^1];
            }

            var leftNode = (ChoiceNode)result.Children[1];
            uint? leftPrecedence;
            if (uint.TryParse(leftNode.Node.Value, out uint leftValue))
                leftPrecedence = leftValue;
            else
                leftPrecedence = null;

            var rightNode = (ChoiceNode)result.Children[2];
            uint? rightPrecedence;
            if (uint.TryParse(rightNode.Node.Value, out uint rightValue))
                rightPrecedence = rightValue;
            else
                rightPrecedence = null;

            var patternNode = result.Children[3];
            var operatorInfo = PegHelper.BuildPegPattern(patternNode);

            return new OperatorInfo(name, operatorInfo, leftPrecedence, rightPrecedence);
        }
    }
}
