using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public delegate INode BuildExpressionNode(INode? leftNode, INode? rightNode);

    public class OperatorPattern
    {
        public IPattern Pattern { get; }

        public uint? LeftPrecedence { get; }
        public uint? RightPrecedence { get; }

        public BuildExpressionNode BuildNode { get; }

        public OperatorPattern(
            IPattern pattern,
            uint? leftPrecedence,
            uint? rightPrecedence,
            BuildExpressionNode buildNode)
        {
            Pattern = pattern;

            LeftPrecedence = leftPrecedence;
            RightPrecedence = rightPrecedence;

            BuildNode = buildNode;
        }

        public bool IsNullary() => LeftPrecedence == null && RightPrecedence == null;
        public bool IsPrefix() => LeftPrecedence == null && RightPrecedence != null;
        public bool IsPostfix() => LeftPrecedence != null && RightPrecedence == null;
        public bool IsInfix() => LeftPrecedence != null && RightPrecedence != null;
    }
}
