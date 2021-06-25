using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class AstParser
    {
        private readonly OperatorInfo[] _operators;
        private readonly Lexer _lexer;
        private readonly Stack<NodeBuilder> _nodeBuilders;

        private INode? _operand;

        private AstParser(OperatorInfo[] operators, Lexer lexer)
        {
            _operators = operators;
            _lexer = lexer;
            _nodeBuilders = new Stack<NodeBuilder>();
        }

        public static INode Parse(OperatorInfo[] operators, Lexer lexer)
        {
            var astParser = new AstParser(operators, lexer);
            var node = astParser.Parse();

            if (lexer.Current.Keyword != Keyword.EndOfString)
                throw new UnexpectedTokenException(lexer.Current, Keyword.EndOfString);

            return node;
        }

        private INode Parse()
        {
            while (_lexer.MoveNext())
            {
                var op = TryGetOperator();
                if (op == null)
                {
                    bool acknowledgeToken;
                    do
                    {
                        if (_nodeBuilders.Count == 0)
                        {
                            if (_operand == null)
                                throw new ExpressionTermException(_lexer.Current);

                            return _operand;
                        }
                    } while (TryReduce(out acknowledgeToken) && !acknowledgeToken);

                    if (acknowledgeToken)
                        continue;

                    op = TryGetOperator();

                    if (op == null)
                        throw new ExpressionTermException(_lexer.Current);
                }

                while (_nodeBuilders.TryPeek(out var left) && OperatorInfo.IsEvaluatedBefore(left.OperatorInfo, op))
                {
                    if (!TryReduce(out bool acknowledgeToken))
                        throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                    if (acknowledgeToken)
                        throw new Exception($"Unexpected token acknowledge");
                }

                PushOperator(op);
            }

            throw new ArgumentException(nameof(_lexer));
        }

        private OperatorInfo? TryGetOperator()
        {
            OperatorInfo op;
            if (_operand == null)
                op = _operators.FirstOrDefault(x => x.Keyword == _lexer.Current.Keyword && (x.IsPrefix() || x.IsNullary()));
            else
                op = _operators.FirstOrDefault(x => x.Keyword == _lexer.Current.Keyword && (x.IsInfix() || x.IsPostfix()));

            return op;
        }

        private void PushOperator(OperatorInfo op)
        {
            var nodeBuilder = op.CreateNodeBuilder();
            _operand = nodeBuilder.FeedOperandLeft(_lexer.Current, _operand);

            if (_operand == null)
                _nodeBuilders.Push(nodeBuilder);
        }

        private bool TryReduce(out bool acknowledgeToken)
        {
            var nodeBuilder = _nodeBuilders.Pop();
            _operand = nodeBuilder.FeedOperandRight(_lexer.Current, _operand, out acknowledgeToken);

            if (_operand == null)
            {
                _nodeBuilders.Push(nodeBuilder);
                return false;
            }

            return true;
        }
    }
}
