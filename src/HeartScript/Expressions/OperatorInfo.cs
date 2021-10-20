using HeartScript.Parsing;

namespace HeartScript.Expressions
{
    public class OperatorInfo
    {
        public string? Name { get; }
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
    }
}
