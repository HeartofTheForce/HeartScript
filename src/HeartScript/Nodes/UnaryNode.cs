using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class PrefixNode : INode
    {
        public Keyword Keyword { get; }
        public INode Target { get; }

        public PrefixNode(Keyword keyword, INode target)
        {
            Keyword = keyword;
            Target = target;
        }

        public override string ToString()
        {
            return $"{{{Keyword} {Target}}}";
        }

        public static NodeBuilder Builder(OperatorInfo operatorInfo)
        {
            return new NodeBuilder(
                operatorInfo,
                1,
                null,
                null,
                (token, leftNode, rightNodes) => new PostfixNode(token.Keyword, rightNodes[0]));
        }
    }

    public class PostfixNode : INode
    {
        public Keyword Keyword { get; }
        public INode Target { get; }

        public PostfixNode(Keyword keyword, INode target)
        {
            Keyword = keyword;
            Target = target;
        }

        public override string ToString()
        {
            return $"{{{Keyword} {Target}}}";
        }

        public static NodeBuilder Builder(OperatorInfo operatorInfo)
        {
            return new NodeBuilder(
                operatorInfo,
                0,
                null,
                null,
                (token, leftNode, rightNodes) => new PostfixNode(token.Keyword, leftNode!));
        }
    }
}
