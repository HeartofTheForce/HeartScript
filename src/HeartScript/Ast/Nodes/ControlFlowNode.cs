namespace HeartScript.Ast.Nodes
{
    public class BreakNode : AstNode
    {
        public BreakNode() : base(typeof(void), AstType.Default)
        {
        }
    }

    public class ContinueNode : AstNode
    {
        public ContinueNode() : base(typeof(void), AstType.Default)
        {
        }
    }

    public partial class AstNode
    {
        public static BreakNode Break() => new BreakNode();
        public static ContinueNode Continue() => new ContinueNode();
    }
}
