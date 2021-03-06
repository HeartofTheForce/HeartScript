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

    public class CallNodeBuilder : NodeBuilder
    {
        private INode? _target;
        private readonly List<INode> _parameters;

        public CallNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
            _parameters = new List<INode>();
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (_target == null)
            {
                if (operand == null)
                    throw new System.ArgumentException($"{nameof(operand)}");

                _target = operand;

                acknowledgeToken = false;
                return null;
            }

            if (current.Keyword == Keyword.RoundClose)
            {
                if (_target == null)
                    throw new System.ArgumentException($"{nameof(_target)}");

                if (operand != null)
                    _parameters.Add(operand);

                acknowledgeToken = true;
                return new CallNode(_target, _parameters);
            }

            if (operand == null)
                throw new System.ArgumentException($"{nameof(operand)}");

            if (current.Keyword == Keyword.Comma)
            {
                _parameters.Add(operand);

                acknowledgeToken = true;
                return null;
            }
            else
            {
                if (_target == null)
                    throw new System.ArgumentException($"{nameof(_target)}");

                acknowledgeToken = false;
                return ErrorNode.UnexpectedToken(this, current, Keyword.RoundClose);
            }
        }
    }
}
