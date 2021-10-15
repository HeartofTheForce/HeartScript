using HeartScript.Parsing;

namespace HeartScript.Expressions
{
    public class OperatorInfo
    {
        public string? Name {get;}
        public IPattern Pattern { get; }

        public uint? LeftPrecedence { get; }
        public uint? RightPrecedence { get; }

        public OperatorInfo(
            string? name,
            IPattern pattern,
            uint? leftPrecedence,
            uint? rightPrecedence)
        {
            Name = name;
            Pattern = pattern;
            LeftPrecedence = leftPrecedence;
            RightPrecedence = rightPrecedence;
        }

        public bool IsNullary() => LeftPrecedence == null && RightPrecedence == null;
        public bool IsPrefix() => LeftPrecedence == null && RightPrecedence != null;
        public bool IsPostfix() => LeftPrecedence != null && RightPrecedence == null;
        public bool IsInfix() => LeftPrecedence != null && RightPrecedence != null;

        public static bool IsEvaluatedBefore(OperatorInfo left, OperatorInfo right)
        {
            if (left.RightPrecedence == null || right.LeftPrecedence == null)
                return left.RightPrecedence == null;

            return left.RightPrecedence <= right.LeftPrecedence;
        }
    }
}
