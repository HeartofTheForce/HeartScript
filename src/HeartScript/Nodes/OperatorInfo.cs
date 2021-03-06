using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class OperatorInfo
    {
        public Keyword Keyword { get; }
        public uint? LeftPrecedence { get; }
        public uint? RightPrecedence { get; }

        public OperatorInfo(
            Keyword keyword,
            uint? leftPrecedence,
            uint? rightPrecedence)
        {
            Keyword = keyword;
            LeftPrecedence = leftPrecedence;
            RightPrecedence = rightPrecedence;
        }

        public bool IsNullary() => LeftPrecedence == null && RightPrecedence == null;
        public bool IsPrefix() => LeftPrecedence == null && RightPrecedence != null;
        public bool IsPostfix() => LeftPrecedence != null && RightPrecedence == null;
        public bool IsInfix() => LeftPrecedence != null && RightPrecedence != null;

        public static bool IsEvaluatedBefore(OperatorInfo left, OperatorInfo right)
        {
            if (left.RightPrecedence == null || right.LeftPrecedence == null)
                return left.RightPrecedence == null;

            return left.RightPrecedence <= right.LeftPrecedence;
        }
    }

    public class Operator
    {
        public OperatorInfo OperatorInfo { get; }
        private readonly Func<OperatorInfo, Token, INode?, INodeBuilder> _createNodeBuilder;

        public Operator(OperatorInfo operatorInfo, Func<OperatorInfo, Token, INode?, INodeBuilder> createNodeBuilder)
        {
            OperatorInfo = operatorInfo;
            _createNodeBuilder = createNodeBuilder;
        }

        public INodeBuilder CreateNodeBuilder(Token token, INode? operand) => _createNodeBuilder(OperatorInfo, token, operand);
    }
}
