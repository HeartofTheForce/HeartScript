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
        private readonly Stack<INodeBuilder> _nodeBuilders;

        private INode? _operand;

        public AstParser(Operator[] operators, IEnumerator<Token> tokens)
        {
            _operators = operators;
            _tokens = tokens;
            _nodeBuilders = new Stack<INodeBuilder>();
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
                    while (true)
                    {
                        if (_nodeBuilders.Count == 0)
                            throw new Exception($"Unexpected Token: {current}");

                        var nodeBuilder = _nodeBuilders.Pop();
                        nodeBuilder.FeedOperand(_operand!);

                        if (!nodeBuilder.IsComplete())
                        {
                            nodeBuilder.AllowUnexpectedToken(_tokens);
                            if (nodeBuilder.IsComplete())
                                _operand = nodeBuilder.Build();
                            else
                            {
                                _nodeBuilders.Push(nodeBuilder);
                                _operand = null;
                            }

                            break;
                        }
                        else
                        {
                            _operand = nodeBuilder.Build();
                        }
                    }
                }
                else
                {
                    while (_nodeBuilders.TryPeek(out var left) && OperatorInfo.IsEvaluatedBefore(left.OperatorInfo, op.OperatorInfo))
                    {
                        PopNodeBuilder();
                    }

                    var nodeBuilder = op.CreateNodeBuilder(current, _operand);
                    if (nodeBuilder.IsComplete())
                        _operand = nodeBuilder.Build();
                    else
                    {
                        _nodeBuilders.Push(nodeBuilder);
                        _operand = null;
                    }
                }
            }

            return Reduce();
        }

        private void PopNodeBuilder()
        {
            var nodeBuilder = _nodeBuilders.Pop();

            nodeBuilder.FeedOperand(_operand!);
            if (!nodeBuilder.IsComplete())
                throw new Exception($"{nameof(NodeBuilder)} is incomplete");

            _operand = nodeBuilder.Build();
        }

        private INode Reduce()
        {
            while (_nodeBuilders.Count > 0)
            {
                PopNodeBuilder();
            }

            if (_nodeBuilders.Count != 0)
                throw new Exception($"Expected 0 {nameof(NodeBuilder)}");

            return _operand!;
        }
    }
}
