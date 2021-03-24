using HeartScript.Nodes;
using HeartScript.Parsing;

namespace HeartScript
{
    public static class Demo
    {
        public static readonly OperatorInfo[] Operators = new OperatorInfo[]
        {
            ConstantNode.OperatorInfo(),
            IdentifierNode.OperatorInfo(),
            BracketNode.OperatorInfo(Keyword.RoundOpen, Keyword.RoundClose),
            CallNode.OperatorInfo(),
            TernaryNode.OperatorInfo(),
            PostfixNode.OperatorInfo(Keyword.Factorial, 1),
            PrefixNode.OperatorInfo(Keyword.Plus, 0),
            PrefixNode.OperatorInfo(Keyword.Minus, 0),
            PrefixNode.OperatorInfo(Keyword.BitwiseNot, 0),
            BinaryNode.OperatorInfo(Keyword.Multiply, 1, 1),
            BinaryNode.OperatorInfo(Keyword.Divide, 1, 1),
            BinaryNode.OperatorInfo(Keyword.Plus, 2, 2),
            BinaryNode.OperatorInfo(Keyword.Minus, 2, 2),
            BinaryNode.OperatorInfo(Keyword.BitwiseAnd, 3, 3),
            BinaryNode.OperatorInfo(Keyword.BitwiseXor, 4, 4),
            BinaryNode.OperatorInfo(Keyword.BitwiseOr, 5, 5),
        };
    }
}
