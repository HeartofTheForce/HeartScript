using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class CallNode : INode
    {
        public Keyword Keyword { get; }
        public INode Target { get; }
        public IEnumerable<INode> Parameters { get; }

        public CallNode(Keyword keyword, INode target, IEnumerable<INode> parameters)
        {
            Keyword = keyword;
            Target = target;
            Parameters = parameters;
        }

        public override string ToString()
        {
            string parameters = string.Join(' ', Parameters);
            return $"{{{Keyword} {parameters}}}";
        }

        public static NodeBuilder Builder(OperatorInfo operatorInfo)
        {
            return new NodeBuilder(
                operatorInfo,
                null,
                Keyword.Comma,
                Keyword.RoundClose,
                (token, leftNode, rightNodes) => new CallNode(token.Keyword, leftNode!, rightNodes));
        }
    }
}
