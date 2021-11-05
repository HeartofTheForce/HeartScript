namespace HeartScript.Ast.Nodes
{
    public class ReturnNode : AstNode
    {
        public AstNode? Node { get; }

        public ReturnNode(AstNode node) : base(node.Type, AstType.Default)
        {
            Node = node;
        }

        public ReturnNode() : base(typeof(void), AstType.Default)
        {
        }
    }

    public partial class AstNode
    {
        public static ReturnNode Return(AstNode node) => new ReturnNode(node);
        public static ReturnNode Return() => new ReturnNode();
    }
}
