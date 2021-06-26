using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public static class BracketNode
    {
        public static OperatorInfo OperatorInfo(LexerPattern open, LexerPattern close)
        {
            return new OperatorInfo(
                open,
                null,
                uint.MaxValue,
                1,
                null,
                close,
                (token, leftNode, rightNodes) => rightNodes[0]);
        }
    }
}
