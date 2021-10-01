using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class ExpressionParser
    {
        private readonly IEnumerable<OperatorInfo> _operators;
        private readonly Lexer _lexer;
        private readonly Stack<NodeBuilder> _nodeBuilders;

        private INode? _operand;
        private Token? _currentToken;

        private ExpressionParser(IEnumerable<OperatorInfo> operators, Lexer lexer)
        {
            _operators = operators;
            _lexer = lexer;
            _nodeBuilders = new Stack<NodeBuilder>();
        }

        public static INode Parse(IEnumerable<OperatorInfo> operators, Lexer lexer)
        {
            var expressionParser = new ExpressionParser(operators, lexer);
            var node = expressionParser.Parse();

            if (!lexer.IsEOF)
                throw new UnexpectedTokenException(lexer.Offset, "EOF");

            return node;
        }

        private INode Parse()
        {
            while (true)
            {
                var op = TryGetOperator();
                if (op == null)
                {
                    int initialOffset = _lexer.Offset;
                    do
                    {
                        if (_nodeBuilders.Count == 0)
                        {
                            if (_operand == null)
                                throw new ExpressionTermException(_lexer.Offset);

                            return _operand;
                        }
                    } while (TryReduce() && initialOffset == _lexer.Offset);

                    if (initialOffset != _lexer.Offset)
                        continue;

                    op = TryGetOperator();

                    if (op == null)
                        throw new ExpressionTermException(_lexer.Offset);
                }

                while (_nodeBuilders.TryPeek(out var left) && left.IsEvaluatedBefore(op))
                {
                    if (!TryReduce())
                        throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                }

                PushOperator(op);
            }

            throw new ArgumentException(nameof(_lexer));
        }

        private OperatorInfo? TryGetOperator()
        {
            OperatorInfo op;
            if (_operand == null)
                op = _operators.FirstOrDefault(x => (x.IsPrefix() || x.IsNullary()) && _lexer.TryEat(x.Keyword, out _currentToken));
            else
                op = _operators.FirstOrDefault(x => (x.IsInfix() || x.IsPostfix()) && _lexer.TryEat(x.Keyword, out _currentToken));

            return op;
        }

        private void PushOperator(OperatorInfo op)
        {
            var nodeBuilder = op.CreateNodeBuilder();
            _operand = nodeBuilder.FeedOperandLeft(_lexer, _currentToken!, _operand);

            if (_operand == null)
                _nodeBuilders.Push(nodeBuilder);
        }

        private bool TryReduce()
        {
            var nodeBuilder = _nodeBuilders.Pop();
            _operand = nodeBuilder.FeedOperandRight(_lexer, _operand);

            if (_operand == null)
            {
                _nodeBuilders.Push(nodeBuilder);
                return false;
            }

            return true;
        }
    }
}
