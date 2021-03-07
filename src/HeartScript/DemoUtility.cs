using HeartScript.Nodes;
using HeartScript.Parsing;

namespace HeartScript
{
    public static class DemoUtility
    {
        public static readonly Operator[] Operators = new Operator[]
        {
            new Operator(
                new OperatorInfo(Keyword.StartOfString, null, int.MaxValue),
                (operatorInfo) => BracketNode.Builder(operatorInfo, Keyword.EndOfString)),
            new Operator(
                new OperatorInfo(Keyword.Constant, null, null),
                ConstantNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.Identifier, null, null),
                IdentifierNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.RoundOpen, null, int.MaxValue),
                (operatorInfo) => BracketNode.Builder(operatorInfo, Keyword.RoundClose)),
            new Operator(
                new OperatorInfo(Keyword.RoundOpen, int.MaxValue - 1, int.MaxValue),
                CallNode.Builder),

            new Operator(
                new OperatorInfo(Keyword.Factorial, 1, null),
                PostfixNode.Builder),

            new Operator(
                new OperatorInfo(Keyword.Plus, null, 0),
                PrefixNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.Minus, null, 0),
                PrefixNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.BitwiseNot, null, 0),
                PrefixNode.Builder),

            new Operator(
                new OperatorInfo(Keyword.Multiply, 1, 1),
                BinaryNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.Divide, 1, 1),
                BinaryNode.Builder),

            new Operator(
                new OperatorInfo(Keyword.Plus, 2, 2),
                BinaryNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.Minus, 2, 2),
                BinaryNode.Builder),

            new Operator(
                new OperatorInfo(Keyword.BitwiseAnd, 3, 3),
                BinaryNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.BitwiseXor, 4, 4),
                BinaryNode.Builder),
            new Operator(
                new OperatorInfo(Keyword.BitwiseOr, 5, 5),
                BinaryNode.Builder),
    };
    }
}
