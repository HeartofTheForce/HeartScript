using System;
using System.Collections.Generic;

namespace HeartScript.Parsing
{
    public class PatternBuilder
    {
        public Dictionary<string, Func<PatternBuilder, INode, IPattern>> Builders { get; }

        public PatternBuilder()
        {
            Builders = new Dictionary<string, Func<PatternBuilder, INode, IPattern>>();
        }
    }
}
