using HeartScript.Nodes;
using HeartScript.Parsing;

namespace HeartScript
{
    public static class DemoUtility
    {
        public static readonly Operator[] Operators = new Operator[]
        {
            new Operator(
                new OperatorInfo(Keyword.Constant, null, null),
                (operatorInfo, token, left) => new ConstantNodeBuilder(operatorInfo, token, left!)),
            new Operator(
                new OperatorInfo(Keyword.Identifier, null, null),
                (operatorInfo, token, left) => new IdentifierNodeBuilder(operatorInfo, token, left!)),
            new Operator(
                new OperatorInfo(Keyword.RoundOpen, null, int.MaxValue),
                (operatorInfo, token, left) => new BracketNodeBuilder(operatorInfo, token, left!, Keyword.RoundClose)),
            new Operator(
                new OperatorInfo(Keyword.RoundOpen, int.MaxValue - 1, int.MaxValue),
                (operatorInfo, token, left) => new CallNodeBuilder(operatorInfo, token, left!)),

            new Operator(
                new OperatorInfo(Keyword.Plus, null, 0),
                (operatorInfo, token, left) => new UnaryNodeBuilder(operatorInfo, token, left!)),
            new Operator(
                new OperatorInfo(Keyword.Minus, null, 0),
                (operatorInfo, token, left) => new UnaryNodeBuilder(operatorInfo, token, left!)),

            new Operator(
                new OperatorInfo(Keyword.Asterisk, 1, 1),
                (operatorInfo, token, left) => new BinaryNodeBuilder(operatorInfo, token, left!)),
            new Operator(
                new OperatorInfo(Keyword.ForwardSlash, 1, 1),
                (operatorInfo, token, left) => new BinaryNodeBuilder(operatorInfo, token, left!)),

            new Operator(
                new OperatorInfo(Keyword.Plus, 2, 2),
                (operatorInfo, token, left) => new BinaryNodeBuilder(operatorInfo, token, left!)),
            new Operator(
                new OperatorInfo(Keyword.Minus, 2, 2),
                (operatorInfo, token, left) => new BinaryNodeBuilder(operatorInfo, token, left!)),
    };
    }
}
