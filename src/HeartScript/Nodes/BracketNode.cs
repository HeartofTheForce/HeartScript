using System.Collections.Generic;
using System.Linq;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class BracketNode : INode
    {
        public Keyword Keyword { get; }
        public INode Target { get; }

        public BracketNode(Keyword keyword, INode target)
        {
            Keyword = keyword;
            Target = target;
        }
    }

    public class BracketNodeBuilder : NodeBuilder, INodeBuilder
    {
        private bool _isComplete;
        private readonly Keyword _closingKeyword;

        public BracketNodeBuilder(OperatorInfo operatorInfo, Token token, INode leftNode, Keyword closingKeyword) : base(operatorInfo, token, leftNode)
        {
            _closingKeyword = closingKeyword;
        }

        public bool IsComplete() => _isComplete;

        public INode Build()
        {
            return new BracketNode(Token.Keyword, RightNodes.Single());
        }

        public override bool AllowUnexpectedToken(IEnumerator<Token> tokens)
        {
            if (tokens.Current.Keyword == _closingKeyword)
            {
                _isComplete = true;
                return true;
            }
            else
                return base.AllowUnexpectedToken(tokens);
        }
    }
}
