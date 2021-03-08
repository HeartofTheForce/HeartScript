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

        public static OperatorInfo OperatorInfo(Keyword keyword, uint rightPrecedence)
        {
            return new OperatorInfo(
                keyword,
                null,
                rightPrecedence,
                1,
                null,
                null,
                (token, leftNode, rightNodes) => new PrefixNode(token.Keyword, rightNodes[0]));
        }
    }
}
