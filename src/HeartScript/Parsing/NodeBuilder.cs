using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class NodeBuilder
    {
        private const bool AllowTrailingDelimiter = false;

        private readonly OperatorInfo _operatorInfo;
        private Token? _token;
        private INode? _leftNode;
        private readonly List<INode> _rightNodes;

        public NodeBuilder(OperatorInfo operatorInfo)
        {
            _operatorInfo = operatorInfo;
            _rightNodes = new List<INode>();
        }

        public INode? FeedOperandLeft(Lexer lexer, INode? operand)
        {
            _token = lexer.Current;

            if (_operatorInfo.LeftPrecedence == null != (operand == null))
                throw new ArgumentException(nameof(operand));

            _leftNode = operand;

            if (_operatorInfo.RightOperands == 0)
            {
                if (_operatorInfo.Terminator == null || lexer.Eat(_operatorInfo.Terminator))
                    return _operatorInfo.BuildNode(_token, _leftNode, _rightNodes);
            }

            return null;
        }

        public INode? FeedOperandRight(Lexer lexer, INode? operand)
        {
            if (_token == null)
                throw new ArgumentException(nameof(_token));

            int initialOffset = lexer.Offset;

            if (operand != null)
                _rightNodes.Add(operand);

            if (_operatorInfo.ExpectTerminator(_rightNodes.Count) && _operatorInfo.Terminator != null)
            {
                if (!AllowTrailingDelimiter && _rightNodes.Count > 0 && operand == null)
                    throw new ExpressionTermException(initialOffset);

                if (lexer.Eat(_operatorInfo.Terminator))
                    return _operatorInfo.BuildNode(_token, _leftNode, _rightNodes);
            }

            if (_operatorInfo.ExpectDelimiter(_rightNodes.Count))
            {
                if (_operatorInfo.Delimiter == null || lexer.Eat(_operatorInfo.Delimiter))
                {
                    if (operand == null)
                        throw new ExpressionTermException(initialOffset);

                    return null;
                }
            }

            if (_operatorInfo.ExpectTerminator(_rightNodes.Count) && _operatorInfo.Terminator == null)
            {
                if (_rightNodes.Count > 0 && operand == null)
                    throw new ExpressionTermException(initialOffset);

                return _operatorInfo.BuildNode(_token, _leftNode, _rightNodes);
            }

            if (_operatorInfo.Terminator != null)
                throw new UnexpectedTokenException(initialOffset, _operatorInfo.Terminator);
            if (_operatorInfo.Delimiter != null)
                throw new UnexpectedTokenException(initialOffset, _operatorInfo.Delimiter);

            throw new Exception($"{nameof(NodeBuilder)} unhandled state");
        }

        public bool IsEvaluatedBefore(OperatorInfo right)
        {
            if (_operatorInfo.Terminator != null)
                return false;

            if (!_operatorInfo.ExpectTerminator(_rightNodes.Count + 1))
                return false;

            return _operatorInfo.RightPrecedence <= right.LeftPrecedence;
        }
    }
}
