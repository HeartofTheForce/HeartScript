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
                (operatorInfo) => new BracketNodeBuilder(operatorInfo,Keyword.EndOfString)),
            new Operator(
                new OperatorInfo(Keyword.Constant, null, null),
                (operatorInfo) => new ConstantNodeBuilder(operatorInfo)),
            new Operator(
                new OperatorInfo(Keyword.Identifier, null, null),
                (operatorInfo) => new IdentifierNodeBuilder(operatorInfo)),
            new Operator(
                new OperatorInfo(Keyword.RoundOpen, null, int.MaxValue),
                (operatorInfo) => new BracketNodeBuilder(operatorInfo, Keyword.RoundClose)),
            new Operator(
                new OperatorInfo(Keyword.RoundOpen, int.MaxValue - 1, int.MaxValue),
                (operatorInfo) => new CallNodeBuilder(operatorInfo)),

            new Operator(
                new OperatorInfo(Keyword.Factorial, 1, null),
                (operatorInfo) => new PostfixNodeBuilder(operatorInfo)),

            new Operator(
                new OperatorInfo(Keyword.Plus, null, 0),
                (operatorInfo) => new PrefixNodeBuilder(operatorInfo)),
            new Operator(
                new OperatorInfo(Keyword.Minus, null, 0),
                (operatorInfo) => new PrefixNodeBuilder(operatorInfo)),

            new Operator(
                new OperatorInfo(Keyword.Multiply, 1, 1),
                (operatorInfo) => new BinaryNodeBuilder(operatorInfo)),
            new Operator(
                new OperatorInfo(Keyword.Divide, 1, 1),
                (operatorInfo) => new BinaryNodeBuilder(operatorInfo)),

            new Operator(
                new OperatorInfo(Keyword.Plus, 2, 2),
                (operatorInfo) => new BinaryNodeBuilder(operatorInfo)),
            new Operator(
                new OperatorInfo(Keyword.Minus, 2, 2),
                (operatorInfo) => new BinaryNodeBuilder(operatorInfo)),
    };
    }
}
