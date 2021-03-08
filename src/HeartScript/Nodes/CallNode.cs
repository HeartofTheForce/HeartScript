using System.Collections.Generic;
using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class CallNode : INode
    {
        public Token Token { get; }
        public INode Target { get; }
        public IEnumerable<INode> Parameters { get; }

        public CallNode(Token token, INode target, IEnumerable<INode> parameters)
        {
            Token = token;
            Target = target;
            Parameters = parameters;
        }

        public override string ToString()
        {
            string parameters = string.Join(' ', Parameters);
            return $"(Call {Target} {parameters})";
        }

        public static OperatorInfo OperatorInfo()
        {
            return new OperatorInfo(
                Keyword.RoundOpen,
                uint.MaxValue - 1,
                uint.MaxValue,
                null,
                Keyword.Comma,
                Keyword.RoundClose,
                (token, leftNode, rightNodes) => new CallNode(token, leftNode!, rightNodes));
        }
    }
}
