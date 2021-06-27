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
            BracketNode.OperatorInfo(new LexerPattern("(", false), new LexerPattern(")", false)),
            CallNode.OperatorInfo(),
            TernaryNode.OperatorInfo(),
            PostfixNode.OperatorInfo(new LexerPattern("!", false), 2),
            PrefixNode.OperatorInfo(new LexerPattern("+", false), 1),
            PrefixNode.OperatorInfo(new LexerPattern("-", false), 1),
            PrefixNode.OperatorInfo(new LexerPattern("~", false), 1),
            BinaryNode.OperatorInfo(new LexerPattern("*", false), 3, 3),
            BinaryNode.OperatorInfo(new LexerPattern("/", false), 3, 3),
            BinaryNode.OperatorInfo(new LexerPattern("+", false), 4, 4),
            BinaryNode.OperatorInfo(new LexerPattern("-", false), 4, 4),
            BinaryNode.OperatorInfo(new LexerPattern("&", false), 5, 5),
            BinaryNode.OperatorInfo(new LexerPattern("^", false), 6, 6),
            BinaryNode.OperatorInfo(new LexerPattern("|", false), 7, 7),
            ConstantNode.OperatorInfo(),
            IdentifierNode.OperatorInfo(),
        };
    }
}
