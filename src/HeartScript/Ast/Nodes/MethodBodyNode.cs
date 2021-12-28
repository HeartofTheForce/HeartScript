namespace HeartScript.Ast.Nodes
{
    public class MethodBodyNode : AstNode
    {
        public VariableNode[] Variables { get; }
        public BlockNode Body { get; }

        public MethodBodyNode(
            VariableNode[] variables,
            BlockNode body)
        : base(typeof(void), AstType.Default)
        {
            Variables = variables;
            Body = body;
        }
    }
}
