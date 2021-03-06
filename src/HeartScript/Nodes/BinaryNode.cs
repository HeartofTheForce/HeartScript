using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class BinaryNode : INode
    {
        public Keyword Keyword { get; }
        public INode Left { get; }
        public INode Right { get; }

        public BinaryNode(Keyword keyword, INode left, INode right)
        {
            Keyword = keyword;
            Left = left;
            Right = right;
        }
    }

    public class BinaryNodeBuilder : NodeBuilder
    {
        private readonly List<INode> _nodes;

        public BinaryNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
            _nodes = new List<INode>();
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            acknowledgeToken = false;

            if (operand == null)
                return ErrorNode.InvalidExpressionTerm(current.CharOffset, current.Keyword);

            _nodes.Add(operand);
            if (_nodes.Count == 2)
                return new BinaryNode(OperatorInfo.Keyword, _nodes[0], _nodes[1]);
            else
                return null;
        }
    }
}
