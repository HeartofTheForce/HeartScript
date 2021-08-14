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

            Regex temp;
            if (IsRegex)
                temp = new Regex(pattern);
            else
                temp = new Regex(Regex.Escape(pattern));

            Regex = new Regex($"\\G({temp})");
        }

        public override string ToString()
        {
            if (IsRegex)
                return $"Regex: {Pattern}";
            else
                return Pattern;
        }
    }
}
