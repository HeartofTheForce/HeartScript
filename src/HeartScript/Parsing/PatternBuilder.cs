using System;
using System.Collections.Generic;
#pragma warning disable IDE0066

namespace HeartScript.Parsing
{
    public class PatternBuilder
    {
        public Dictionary<string, Func<PatternBuilder, INode, IPattern>> Builders { get; }

        public PatternBuilder()
        {
            Builders = new Dictionary<string, Func<PatternBuilder, INode, IPattern>>();
        }

        public IPattern BuildPattern(string key, INode node)
        {
            return Builders[key](this, node);
        }
    }
}
