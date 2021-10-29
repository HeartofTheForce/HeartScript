using HeartScript.Parsing;

namespace HeartScript.Expressions
{
    public class OperatorInfo
    {
        public string Key { get; }
        public IPattern Pattern { get; }

        public uint? LeftPrecedence { get; }
        public uint? RightPrecedence { get; }

        public OperatorInfo(
            string key,
            IPattern pattern,
            uint? leftPrecedence,
            uint? rightPrecedence)
        {
            Key = key;
            Pattern = pattern;
            LeftPrecedence = leftPrecedence;
            RightPrecedence = rightPrecedence;
        }
    }
}
