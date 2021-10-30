namespace HeartScript.Ast.Nodes
{
    public class UnaryNode : AstNode
    {
        public AstNode Operand { get; }

        public UnaryNode(AstNode operand, AstType nodeType) : base(operand.Type, nodeType)
        {
            Operand = operand;
        }
    }
}
