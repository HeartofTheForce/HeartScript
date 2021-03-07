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
                else if (_parameters.Count != 0)
                    throw new ExpressionTermException(current);

                acknowledgeToken = true;
                return new CallNode(OperatorInfo.Keyword, _target, _parameters);
            }

            if (operand == null)
                throw new ExpressionTermException(current);

            if (current.Keyword == Keyword.Comma)
            {
                _parameters.Add(operand);

                acknowledgeToken = true;
                return null;
            }
            else
                throw new UnexpectedTokenException(current, Keyword.RoundClose);
        }
    }
}
