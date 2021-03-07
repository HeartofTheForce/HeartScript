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
    }

    public class PrefixNodeBuilder : NodeBuilder
    {
        public PrefixNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (current.Keyword == OperatorInfo.Keyword && operand == null)
            {
                acknowledgeToken = false;
                return null;
            }

            if (operand == null)
                throw new ExpressionTermException(current);

            acknowledgeToken = false;
            return new PrefixNode(OperatorInfo.Keyword, operand);
        }
    }

    public class PostfixNodeBuilder : NodeBuilder
    {
        public PostfixNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (operand == null)
                throw new ExpressionTermException(current);

            acknowledgeToken = false;
            return new PostfixNode(OperatorInfo.Keyword, operand);
        }
    }
}
