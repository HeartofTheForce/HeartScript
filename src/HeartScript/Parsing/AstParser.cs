using System;
using System.Collections.Generic;
using System.Linq;
using HeartScript.Nodes;

namespace HeartScript.Parsing
{
    public class AstParser
    {
        private readonly Operator[] _operators;
        private readonly IEnumerator<Token> _tokens;
        private readonly Stack<NodeBuilder> _nodeBuilders;

        private INode? _operand;

        public AstParser(Operator[] operators, IEnumerator<Token> tokens)
        {
            _operators = operators;
            _tokens = tokens;
            _nodeBuilders = new Stack<NodeBuilder>();
        }

        public static INode Parse(Operator[] operators, IEnumerable<Token> tokens)
        {
            var astParser = new AstParser(operators, tokens.GetEnumerator());
            return astParser.Parse();
        }

        private INode Parse()
        {
            while (_tokens.MoveNext())
            {
                var current = _tokens.Current;

                Operator op;
                if (_operand == null)
                    op = _operators.FirstOrDefault(x => x.OperatorInfo.Keyword == current.Keyword && (x.OperatorInfo.IsPrefix() || x.OperatorInfo.IsNullary()));
                else
                    op = _operators.FirstOrDefault(x => x.OperatorInfo.Keyword == current.Keyword && (x.OperatorInfo.IsInfix() || x.OperatorInfo.IsPostfix()));

                if (op == null)
                {
                    bool acknowledgeToken;
                    do
                    {
                        if (_nodeBuilders.Count == 0)
                            if (_operand != null)
                                return _operand;
                            else
                                return ErrorNode.UnexpectedToken(current);

                        if (!TryPopNodeBuilder(out acknowledgeToken) && !acknowledgeToken)
                            return ErrorNode.UnexpectedToken(current);

                    } while (!acknowledgeToken);
                }
                else
                {
                    bool acknowledgeToken;
                    while (_nodeBuilders.TryPeek(out var left) && OperatorInfo.IsEvaluatedBefore(left.OperatorInfo, op.OperatorInfo))
                    {
                        if (!TryPopNodeBuilder(out acknowledgeToken))
                            throw new Exception($"{nameof(NodeBuilder)} is incomplete");
                        if (acknowledgeToken)
                            throw new Exception($"Unexpected token acknowledge");
                    }

                    var nodeBuilder = op.CreateNodeBuilder();
                    _operand = nodeBuilder.FeedOperand(current, _operand, out acknowledgeToken);

                    if (nodeBuilder.OperatorInfo.IsNullary() && !acknowledgeToken)
                        throw new Exception($"Unexpected token acknowledge");

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
            _operand = nodeBuilder.FeedOperand(_tokens.Current, _operand, out acknowledgeToken);

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
