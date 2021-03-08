using HeartScript.Parsing;

namespace HeartScript.Nodes
{

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

        public static OperatorInfo OperatorInfo(Keyword keyword, uint leftPrecedence)
        {
            return new OperatorInfo(
                keyword,
                leftPrecedence,
                0,
                0,
                null,
                null,
                (token, leftNode, rightNodes) => new PostfixNode(token.Keyword, leftNode!));
        }
    }
}
