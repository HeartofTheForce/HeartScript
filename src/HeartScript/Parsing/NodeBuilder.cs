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
            _token = current;

            if (OperatorInfo.LeftPrecedence == null != (operand == null))
                throw new ArgumentException(nameof(operand));

            _leftNode = operand;

            if (OperatorInfo.RightOperands == 0)
                return OperatorInfo.BuildNode(_token, _leftNode, _rightNodes);

            return null;
        }

        public INode? FeedOperandRight(Lexer lexer, INode? operand)
        {
            if (_token == null)
                throw new ArgumentException(nameof(_token));

            if (operand != null)
                _rightNodes.Add(operand);

            int initialOffset = lexer.Offset;

            if (operand == null && _rightNodes.Count > 0)
                throw new ExpressionTermException(initialOffset);

            bool isTerminator = OperatorInfo.Terminator != null && lexer.Eat(OperatorInfo.Terminator);

            bool isDelimiter =
                !isTerminator &&
                (OperatorInfo.RightOperands == null || _rightNodes.Count < OperatorInfo.RightOperands) &&
                (OperatorInfo.Delimiter == null || lexer.Eat(OperatorInfo.Delimiter));

            if (isDelimiter)
            {
                if (operand == null)
                    throw new ExpressionTermException(initialOffset);

                return null;
            }

            if (OperatorInfo.Terminator == null || isTerminator)
            {
                if (OperatorInfo.RightOperands != null && _rightNodes.Count != OperatorInfo.RightOperands)
                {
                    if (OperatorInfo.Delimiter != null)
                        throw new UnexpectedTokenException(initialOffset, OperatorInfo.Delimiter);
                    else
                        throw new ExpressionTermException(initialOffset);
                }

                return OperatorInfo.BuildNode(_token, _leftNode, _rightNodes);
            }

            throw new UnexpectedTokenException(initialOffset, OperatorInfo.Terminator);
        }
    }
}
