using System.Collections.Generic;

namespace HeartScript.Parsing
{
    public class PatternParser
    {
        public Dictionary<string, IPattern> Patterns { get; }

        public PatternParser()
        {
            Patterns = new Dictionary<string, IPattern>();
        }
    }
}
