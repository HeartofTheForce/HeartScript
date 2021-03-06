using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class CallNode : INode
    {
        public INode Target { get; }
        public IEnumerable<INode> Parameters { get; }

        public CallNode(INode target, IEnumerable<INode> parameters)
        {
            Target = target;
            Parameters = parameters;
        }
    }

    public class CallNodeBuilder : NodeBuilder, INodeBuilder
    {
        private bool _isComplete;

        public CallNodeBuilder(OperatorInfo operatorInfo, Token token, INode leftNode) : base(operatorInfo, token, leftNode)
        {
        }

        public bool IsComplete() => _isComplete;

        public INode Build()
        {
            return new CallNode(LeftNode, RightNodes);
        }

        public override bool AllowUnexpectedToken(IEnumerator<Token> tokens)
        {
            if (tokens.Current.Keyword == Keyword.RoundClose)
            {
                _isComplete = true;
                return true;
            }
            else if (tokens.Current.Keyword == Keyword.Comma)
                return true;
            else
                return base.AllowUnexpectedToken(tokens);
        }
    }
}
