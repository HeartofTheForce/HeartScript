namespace HeartScript.Ast
{
    public class UnaryNode : AstNode
    {
        public AstNode Target { get; }

        public UnaryNode(AstNode target) : base(target.Type)
        {
            Target = target;
        }
    }
}
