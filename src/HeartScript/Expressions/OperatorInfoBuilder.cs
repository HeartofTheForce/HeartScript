using System;
using System.Collections.Generic;
using System.IO;
using HeartScript.Parsing;
using HeartScript.Peg;
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
                .Then(s_name)
                .Discard(PegHelper.NonSignificant)
                .Discard(LexerPattern.FromRegex("->"))
                .Discard(PegHelper.NonSignificant)
                .Then(ChoicePattern.Create()
                    .Or(s_digits)
                    .Or(s_none))
                .Discard(PegHelper.NonSignificant)
                .Then(ChoicePattern.Create()
                    .Or(s_digits)
                    .Or(s_none))
                .Discard(PegHelper.NonSignificant)
                .Then(LookupPattern.Create("peg"));

            var result = pattern.TryMatch(pegParser, ctx);

            if (result == null)
                throw new Exception($"{ctx.Exception}, {lineNumber}");

            var sequenceNode = (SequenceNode)result;

            var keyNode = (ValueNode)sequenceNode.Children[0];
            string key = keyNode.Value[1..^1];

            var leftNode = (ChoiceNode)sequenceNode.Children[1];
            uint? leftPrecedence = null;
            if (leftNode.ChoiceIndex == 0)
            {
                var valueNode = (ValueNode)leftNode.Node;
                leftPrecedence = uint.Parse(valueNode.Value);
            }

            var rightNode = (ChoiceNode)sequenceNode.Children[2];
            uint? rightPrecedence = null;
            if (rightNode.ChoiceIndex == 0)
            {
                var valueNode = (ValueNode)rightNode.Node;
                rightPrecedence = uint.Parse(valueNode.Value);
            }

            var patternNode = sequenceNode.Children[3];
            var operatorInfo = PegHelper.BuildLookup(patternNode);

            return new OperatorInfo(key, operatorInfo, leftPrecedence, rightPrecedence);
        }
    }
}
