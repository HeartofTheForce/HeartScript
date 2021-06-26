using System.Text.RegularExpressions;

namespace HeartScript.Parsing
{
    public class LexerPattern
    {
        public Regex Regex { get; }
        public string Pattern { get; }
        public bool IsRegex { get; }

        public LexerPattern(string pattern, bool isRegex)
        {
            Pattern = pattern;
            IsRegex = isRegex;

            if (IsRegex)
                Regex = new Regex($"\\G\\s*({pattern})");
            else
                Regex = new Regex($"\\G\\s*({Regex.Escape(pattern)})");
        }
    }
}
