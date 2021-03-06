using HeartScript.Parsing;

namespace HeartScript.Nodes
{
    public class RootNodeBuilder : NodeBuilder
    {
        public RootNodeBuilder(OperatorInfo operatorInfo) : base(operatorInfo)
        {
        }

        public override INode? FeedOperand(Token current, INode? operand, out bool acknowledgeToken)
        {
            if (operand == null)
                throw new System.ArgumentException($"{nameof(operand)}");

            acknowledgeToken = true;

            if (current.Keyword != Keyword.EndOfString)
                return ErrorNode.UnexpectedToken(current.CharOffset, Keyword.EndOfString);

            return operand;
        }
    }
}
