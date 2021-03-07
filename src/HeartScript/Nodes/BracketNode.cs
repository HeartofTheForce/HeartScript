using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public static class BracketNode
    {
        public static NodeBuilder Builder(OperatorInfo operatorInfo, Keyword terminator)
        {
            return new NodeBuilder(
                operatorInfo,
                1,
                null,
                terminator,
                (token, leftNode, rightNodes) => rightNodes[0]);
        }
    }
}
