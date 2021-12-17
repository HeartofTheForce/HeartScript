namespace HeartScript.Ast.Nodes
{
    public class BlockNode : AstNode
    {
        public AstNode[] Statements { get; }

        public BlockNode(AstNode[] statements) : base(typeof(void), AstType.Default)
        {
            Statements = statements;
        }
    }

    public partial class AstNode
    {
        public static BlockNode Block(AstNode[] statements) => new BlockNode(statements);
    }
}
