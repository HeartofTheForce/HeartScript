using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public delegate INode BuildNode(Token token, INode? leftNode, IReadOnlyList<INode> rightNodes);

    public class OperatorInfo
    {
        public Keyword Keyword { get; }

        public uint? LeftPrecedence { get; }
        public uint RightPrecedence { get; }
        public uint? RightOperands { get; }

        public Keyword? Delimiter { get; }
        public Keyword? Terminator { get; }

        public BuildNode BuildNode { get; }

        public OperatorInfo(
            Keyword keyword,
            uint? leftPrecedence,
            uint rightPrecedence,
            uint? rightOperands,
            Keyword? delimiter,
            Keyword? terminator,
            BuildNode buildNode)
        {
            Keyword = keyword;
            LeftPrecedence = leftPrecedence;
            RightPrecedence = rightPrecedence;

            if (rightOperands == 0)
            {
                if (delimiter != null)
                    throw new ArgumentException(nameof(delimiter));
                if (terminator != null)
                    throw new ArgumentException(nameof(terminator));
            }

            if (rightOperands == null || rightOperands > 1)
            {
                if (delimiter == null)
                    throw new ArgumentException(nameof(delimiter));
            }

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
