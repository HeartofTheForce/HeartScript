using System;
using System.Collections.Generic;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class NodeBuilder
    {
        private readonly OperatorInfo _operatorInfo;
        private Token? _token;
        private INode? _leftNode;
        private readonly List<INode> _rightNodes;

        public NodeBuilder(OperatorInfo operatorInfo)
        {
            _operatorInfo = operatorInfo;
            _rightNodes = new List<INode>();
        }

        public INode? FeedOperandLeft(Token current, INode? operand)
        {
            _token = current;

            if (_operatorInfo.LeftPrecedence == null != (operand == null))
                throw new ArgumentException(nameof(operand));

            _leftNode = operand;

            if (_operatorInfo.RightOperands == 0)
                return _operatorInfo.BuildNode(_token, _leftNode, _rightNodes);

            return null;
        }

        public INode? FeedOperandRight(Lexer lexer, INode? operand)
        {
            if (_token == null)
                throw new ArgumentException(nameof(_token));

            int initialOffset = lexer.Offset;

            if (operand != null)
                _rightNodes.Add(operand);

            if (operand == null && _rightNodes.Count > 0)
                throw new ExpressionTermException(initialOffset);

            bool expectDelimiter = _operatorInfo.ExpectDelimiter(_rightNodes.Count);
            bool expectTerminator = _operatorInfo.ExpectTerminator(_rightNodes.Count);

            if (expectTerminator)
            {
                if (_operatorInfo.RightOperands != null && _rightNodes.Count != _operatorInfo.RightOperands)
                {
                    if (_operatorInfo.Delimiter != null)
                        throw new UnexpectedTokenException(initialOffset, _operatorInfo.Delimiter);
                    else
                        throw new ExpressionTermException(initialOffset);
                }

                if (_operatorInfo.Terminator == null || lexer.Eat(_operatorInfo.Terminator))
                    return _operatorInfo.BuildNode(_token, _leftNode, _rightNodes);
            }

            if (expectDelimiter)
            {
                if (operand == null)
                    throw new ExpressionTermException(initialOffset);

                if (_operatorInfo.Delimiter == null || lexer.Eat(_operatorInfo.Delimiter))
                    return null;

                if (_operatorInfo.Terminator != null && expectTerminator)
                    throw new UnexpectedTokenException(initialOffset, _operatorInfo.Terminator);

                throw new UnexpectedTokenException(initialOffset, _operatorInfo.Delimiter);
            }

            throw new UnexpectedTokenException(initialOffset, _operatorInfo.Terminator!);
        }

        public bool IsEvaluatedBefore(OperatorInfo target)
        {
            if (_operatorInfo.Terminator != null)
                return false;

            if (!_operatorInfo.ExpectTerminator(_rightNodes.Count + 1))
                return false;

            return OperatorInfo.IsEvaluatedBefore(_operatorInfo, target);
        }
    }
}
