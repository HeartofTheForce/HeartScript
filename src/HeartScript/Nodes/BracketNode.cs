using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public static class BracketNode
    {
        public static OperatorInfo OperatorInfo(Keyword open, Keyword close)
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
