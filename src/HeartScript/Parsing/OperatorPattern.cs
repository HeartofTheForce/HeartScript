using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class OperatorPattern
    {
        public IPattern Pattern { get; }

        public uint? LeftPrecedence { get; }
        public uint? RightPrecedence { get; }

        public OperatorPattern(
            IPattern pattern,
            uint? leftPrecedence,
            uint? rightPrecedence)
        {
            Pattern = pattern;

            LeftPrecedence = leftPrecedence;
            RightPrecedence = rightPrecedence;
        }

        public bool IsNullary() => LeftPrecedence == null && RightPrecedence == null;
        public bool IsPrefix() => LeftPrecedence == null && RightPrecedence != null;
        public bool IsPostfix() => LeftPrecedence != null && RightPrecedence == null;
        public bool IsInfix() => LeftPrecedence != null && RightPrecedence != null;
    }
}