using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public abstract class NodeBuilder
    {
        public OperatorInfo OperatorInfo { get; }
        public Token Token { get; }
        public INode LeftNode { get; }
        public IEnumerable<INode> RightNodes => _rightNodes;

        private readonly List<INode> _rightNodes;

        public NodeBuilder(OperatorInfo operatorInfo, Token token, INode leftNode)
        {
            OperatorInfo = operatorInfo;
            Token = token;
            LeftNode = leftNode;
            _rightNodes = new List<INode>();
        }

        public void FeedOperand(INode operand)
        {
            if (operand == null)
                throw new Exception($"Unexpected null: {nameof(INode)}");

            _rightNodes.Add(operand);
        }

        public virtual bool AllowUnexpectedToken(IEnumerator<Token> tokens) => false;
    }

    public interface INodeBuilder
    {
        OperatorInfo OperatorInfo { get; }
        bool IsComplete();
        void FeedOperand(INode operand);
        bool AllowUnexpectedToken(IEnumerator<Token> tokens);
        INode Build();
    }
}
