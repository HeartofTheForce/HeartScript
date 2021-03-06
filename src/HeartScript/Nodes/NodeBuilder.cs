using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public abstract class NodeBuilder
    {
        public OperatorInfo OperatorInfo { get; }

        public NodeBuilder(OperatorInfo operatorInfo)
        {
            OperatorInfo = operatorInfo;
        }

        public abstract INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken);
    }
}
