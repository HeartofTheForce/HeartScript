using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class AstParser
    {
        private readonly OperatorInfo[] _operators;
        private readonly IEnumerator<Token> _tokens;
        private readonly Stack<NodeBuilder> _nodeBuilders;

        private INode? _operand;

        public AstParser(OperatorInfo[] operators, IEnumerator<Token> tokens)
        {
            _operators = operators;
            _tokens = tokens;
            _nodeBuilders = new Stack<NodeBuilder>();
        }

        public static INode Parse(OperatorInfo[] operators, IEnumerable<Token> tokens)
        {
            var astParser = new AstParser(operators, tokens.GetEnumerator());
            return astParser.Parse();
        }

        private INode Parse()
        {
            while (_tokens.MoveNext())
            {
                var current = _tokens.Current;

                OperatorInfo op;
                if (_operand == null)
                    op = _operators.FirstOrDefault(x => x.Keyword == current.Keyword && (x.IsPrefix() || x.IsNullary()));
                else
                    op = _operators.FirstOrDefault(x => x.Keyword == current.Keyword && (x.IsInfix() || x.IsPostfix()));

                if (op == null)
                {
                    bool acknowledgeToken;
                    do
                    {
                        if (_nodeBuilders.Count == 0)
                            throw new UnexpectedTokenException(current);

                        if (!TryPopNodeBuilder(out acknowledgeToken) && !acknowledgeToken)
                            throw new UnexpectedTokenException(current);

                    } while (!acknowledgeToken);
                }
                else
                {
                    while (_nodeBuilders.TryPeek(out var left) && OperatorInfo.IsEvaluatedBefore(left.OperatorInfo, op))
                    {
                        if (!TryPopNodeBuilder(out bool acknowledgeToken))
                            throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                        if (acknowledgeToken)
                            throw new Exception($"Unexpected token acknowledge");
                    }

                    var nodeBuilder = op.CreateNodeBuilder();
                    _operand = nodeBuilder.FeedOperandLeft(current, _operand);

                    if (_operand == null)
                        _nodeBuilders.Push(nodeBuilder);
                }
            }

            if (_nodeBuilders.Count != 0)
                throw new Exception($"Expected 0 {nameof(NodeBuilder)}");

            return _operand!;
        }

        private bool TryPopNodeBuilder(out bool acknowledgeToken)
        {
            var nodeBuilder = _nodeBuilders.Pop();
            _operand = nodeBuilder.FeedOperandRight(_tokens.Current, _operand, out acknowledgeToken);

            if (_operand == null)
            {
                _nodeBuilders.Push(nodeBuilder);
                return false;
            }

            return true;
        }

        private INode Reduce()
        {
            while (_nodeBuilders.Count > 0)
            {
                if (!TryPopNodeBuilder(out bool acknowledgeToken))
                    throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                if (!acknowledgeToken)
                    throw new Exception($"Unexpected token acknowledge");
            }

            if (_nodeBuilders.Count != 0)
                throw new Exception($"Expected 0 {nameof(NodeBuilder)}");

            return _operand!;
        }
    }
}
