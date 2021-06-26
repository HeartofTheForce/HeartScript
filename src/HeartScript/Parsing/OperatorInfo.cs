using System;
using System.Collections.Generic;
using HeartScript.Nodes;
using static HeartScript.Parsing.Lexer;

namespace HeartScript.Parsing
{
    public delegate INode BuildNode(Token token, INode? leftNode, IReadOnlyList<INode> rightNodes);

    public class OperatorInfo
    {
        public LexerPattern Keyword { get; }

        public uint? LeftPrecedence { get; }
        public uint RightPrecedence { get; }
        public uint? RightOperands { get; }

        public LexerPattern? Delimiter { get; }
        public LexerPattern? Terminator { get; }

        public BuildNode BuildNode { get; }

        public OperatorInfo(
            LexerPattern keyword,
            uint? leftPrecedence,
            uint rightPrecedence,
            uint? rightOperands,
            LexerPattern? delimiter,
            LexerPattern? terminator,
            BuildNode buildNode)
        {

            if (rightOperands == 0)
            {
                if (delimiter != null)
                    throw new ArgumentException(nameof(delimiter));
                if (terminator != null)
                    throw new ArgumentException(nameof(terminator));
            }

            Keyword = keyword;

            LeftPrecedence = leftPrecedence;
            RightPrecedence = rightPrecedence;
            RightOperands = rightOperands;

            Delimiter = delimiter;
            Terminator = terminator;

            BuildNode = buildNode;
        }

        public NodeBuilder CreateNodeBuilder() => new NodeBuilder(this);

        public bool IsNullary() => LeftPrecedence == null && RightOperands == 0;
        public bool IsPrefix() => LeftPrecedence == null && RightOperands != 0;
        public bool IsPostfix() => LeftPrecedence != null && RightOperands == 0;
        public bool IsInfix() => LeftPrecedence != null && RightOperands != 0;

        public static bool IsEvaluatedBefore(OperatorInfo left, OperatorInfo right)
        {
            if (left.RightOperands == 0 || right.LeftPrecedence == null)
                return left.RightOperands == 0;

            return left.RightPrecedence <= right.LeftPrecedence;
        }
    }
}
