using System.Collections.Generic;
using System.Linq;
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
            if (Parameters.Any())
            {
                string parameters = string.Join(' ', Parameters);
                return $"($ {Target} {parameters})";
            }

            return $"($ {Target})";
        }

        public static OperatorInfo OperatorInfo()
        {
            return new OperatorInfo(
                new LexerPattern("(", false),
                0,
                uint.MaxValue,
                null,
                new LexerPattern(",", false),
                new LexerPattern(")", false),
                (token, leftNode, rightNodes) => new CallNode(token, leftNode!, rightNodes));
        }
    }
}
