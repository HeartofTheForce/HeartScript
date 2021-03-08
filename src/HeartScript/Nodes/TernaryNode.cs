using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class TernaryNode : INode
    {
        public INode Target { get; }
        public INode OptionA { get; }
        public INode OptionB { get; }

        public TernaryNode(INode target, INode optionA, INode optionB)
        {
            Target = target;
            OptionA = optionA;
            OptionB = optionB;
        }

        public override string ToString()
        {
            return $"{{{Keyword.Ternary} {OptionA} {OptionB}}}";
        }

        public static NodeBuilder Builder(OperatorInfo operatorInfo)
        {
            return new NodeBuilder(
                operatorInfo,
                2,
                Keyword.Colon,
                null,
                (token, leftNode, rightNodes) => new TernaryNode(leftNode!, rightNodes[0], rightNodes[1]));
        }
    }
}
