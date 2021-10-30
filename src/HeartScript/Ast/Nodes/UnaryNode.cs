namespace HeartScript.Ast.Nodes
{
    public class UnaryNode : AstNode
    {
        public AstNode Operand { get; }

        public UnaryNode(AstNode operand) : base(operand.Type)
        {
            Operand = operand;
        }
    }
}
