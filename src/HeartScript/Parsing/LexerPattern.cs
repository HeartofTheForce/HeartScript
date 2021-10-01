using System.Text.RegularExpressions;

namespace HeartScript.Parsing
{
    public class LexerPattern
    {
        public Regex Regex { get; }
        public string Pattern { get; }
        public bool IsRegex { get; }

        private LexerPattern(string pattern, bool isRegex)
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

        public static LexerPattern FromRegex(string pattern)
        {
            return new LexerPattern(pattern, true);
        }

        public static LexerPattern FromPlainText(string pattern)
        {
            return new LexerPattern(pattern, false);
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
