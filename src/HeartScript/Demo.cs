using HeartScript.Nodes;
using HeartScript.Parsing;

namespace HeartScript
{
    public static class Demo
    {
        public static readonly OperatorInfo[] Operators = new OperatorInfo[]
        {
            IfNode.OperatorInfo(),
            ElseNode.OperatorInfo(),
            ConstantNode.OperatorInfo(),
            IdentifierNode.OperatorInfo(),
            BracketNode.OperatorInfo(Keyword.RoundOpen, Keyword.RoundClose),
            CallNode.OperatorInfo(),
            TernaryNode.OperatorInfo(),
            PostfixNode.OperatorInfo(Keyword.Factorial, 2),
            PrefixNode.OperatorInfo(Keyword.Plus, 1),
            PrefixNode.OperatorInfo(Keyword.Minus, 1),
            PrefixNode.OperatorInfo(Keyword.BitwiseNot, 1),
            BinaryNode.OperatorInfo(Keyword.Multiply, 3, 3),
            BinaryNode.OperatorInfo(Keyword.Divide, 3, 3),
            BinaryNode.OperatorInfo(Keyword.Plus, 4, 4),
            BinaryNode.OperatorInfo(Keyword.Minus, 4, 4),
            BinaryNode.OperatorInfo(Keyword.BitwiseAnd, 5, 5),
            BinaryNode.OperatorInfo(Keyword.BitwiseXor, 6, 6),
            BinaryNode.OperatorInfo(Keyword.BitwiseOr, 7, 7),
        };
    }
}
