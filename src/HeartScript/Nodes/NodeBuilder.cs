using System;
using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public delegate INode BuildNode(Token token, INode? leftNode, IReadOnlyList<INode> rightNodes);

    public class NodeBuilder
    {
        public OperatorInfo OperatorInfo { get; }

        private readonly bool _hasLeftNode;
        private readonly uint? _expectedRightOperands;
        private readonly Keyword? _delimiter;
        private readonly Keyword? _terminator;


        private Token? _token;
        private INode? _leftNode;
        private readonly List<INode> _rightNodes;
        private readonly BuildNode _buildNode;

        public NodeBuilder(
            OperatorInfo operatorInfo,
            uint? expectedRightOperands,
            Keyword? delimiter,
            Keyword? terminator,
            BuildNode buildNode)
        {
            if (expectedRightOperands == 0)
            {
                if (operatorInfo.RightPrecedence != null)
                    throw new ArgumentException(nameof(expectedRightOperands));
                if (delimiter != null)
                    throw new ArgumentException(nameof(delimiter));
                if (terminator != null)
                    throw new ArgumentException(nameof(terminator));
            }

            _hasLeftNode = operatorInfo.LeftPrecedence != null;
            _expectedRightOperands = expectedRightOperands;
            _delimiter = delimiter;
            _terminator = terminator;

            _rightNodes = new List<INode>();
            _buildNode = buildNode;
            OperatorInfo = operatorInfo;
        }

        public INode? FeedOperandLeft(Token current, INode? operand)
        {
            _token = current;

            if (_hasLeftNode)
            {
                if (operand == null)
                    throw new ArgumentException(nameof(operand));

                _leftNode = operand;
            }

            if (_expectedRightOperands == 0)
                return _buildNode(_token, _leftNode, _rightNodes);

            return null;
        }

        public INode? FeedOperandRight(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (operand == null)
                throw new ExpressionTermException(current);

            _rightNodes.Add(operand);

            if (_expectedRightOperands != null && _rightNodes.Count > _expectedRightOperands)
                throw new Exception("Too many right operands");

            if (_terminator == null && _rightNodes.Count == _expectedRightOperands)
            {
                if (_token == null)
                    throw new ArgumentException(nameof(_token));

                acknowledgeToken = false;
                return _buildNode(_token, _leftNode, _rightNodes);
            }

            if (current.Keyword == _delimiter)
            {
                acknowledgeToken = true;
                return null;
            }

            if (current.Keyword == _terminator)
            {
                if (_token == null)
                    throw new ArgumentException(nameof(_token));

                if (_expectedRightOperands != null && _rightNodes.Count < _expectedRightOperands)
                    throw new ExpressionTermException(current);

                acknowledgeToken = true;
                return _buildNode(_token, _leftNode, _rightNodes);
            }

            if (_terminator != null)
                throw new UnexpectedTokenException(current, _terminator.Value);
            else
                throw new UnexpectedTokenException(current);
        }
    }
}
