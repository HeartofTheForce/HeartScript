using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class NodeBuilder
    {
        public OperatorInfo OperatorInfo { get; }

        private Token? _token;
        private INode? _leftNode;
        private readonly List<INode> _rightNodes;

        public NodeBuilder(OperatorInfo operatorInfo)
        {
            OperatorInfo = operatorInfo;
            _rightNodes = new List<INode>();
        }

        public INode? FeedOperandLeft(Token current, INode? operand)
        {
            if (current.Keyword != OperatorInfo.Keyword)
                throw new ArgumentException(nameof(current));

            _token = current;

            if (OperatorInfo.LeftPrecedence != null)
            {
                if (operand == null)
                    throw new ArgumentException(nameof(operand));

                _leftNode = operand;
            }

            if (OperatorInfo.RightOperands == 0)
                return OperatorInfo.BuildNode(_token, _leftNode, _rightNodes);

            return null;
        }

        public INode? FeedOperandRight(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (operand == null)
                throw new ExpressionTermException(current);

            _rightNodes.Add(operand);

            if (OperatorInfo.RightOperands != null && _rightNodes.Count > OperatorInfo.RightOperands)
                throw new ArgumentException(nameof(operand));

            if (current.Keyword == OperatorInfo.Delimiter && (OperatorInfo.RightOperands == null || _rightNodes.Count < OperatorInfo.RightOperands))
            {
                acknowledgeToken = true;
                return null;
            }

            if (OperatorInfo.Terminator == null || current.Keyword == OperatorInfo.Terminator)
            {
                if (_token == null)
                    throw new ArgumentException(nameof(_token));

                if (OperatorInfo.RightOperands != null && _rightNodes.Count < OperatorInfo.RightOperands)
                {
                    if (OperatorInfo.Delimiter != null)
                        throw new UnexpectedTokenException(current, OperatorInfo.Delimiter.Value);
                    else
                        throw new ArgumentException(nameof(OperatorInfo.Delimiter));
                }

                acknowledgeToken = current.Keyword == OperatorInfo.Terminator;
                return OperatorInfo.BuildNode(_token, _leftNode, _rightNodes);
            }

            throw new UnexpectedTokenException(current, OperatorInfo.Terminator.Value);
        }
    }
}
