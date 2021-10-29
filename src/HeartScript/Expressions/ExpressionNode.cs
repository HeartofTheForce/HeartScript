using HeartScript.Compiling;
using HeartScript.Parsing;

namespace HeartScript.Expressions
{
    public class ExpressionNode : IParseNode
    {
        public string Name { get; }
        public int CharIndex { get; set; }

        public ExpressionNode? LeftNode { get; }
        public IParseNode MidNode { get; }
        public ExpressionNode? RightNode { get; }

        public ExpressionNode(string name, ExpressionNode? leftNode, IParseNode midNode, ExpressionNode? rightNode)
        {
            Name = name;

            LeftNode = leftNode;
            MidNode = midNode;
            RightNode = rightNode;

            if (leftNode != null)
                CharIndex = leftNode.CharIndex;
            else
                CharIndex = midNode.CharIndex;
        }

        public override string ToString()
        {
            return StringCompiler.Compile(this);
        }
    }
}
