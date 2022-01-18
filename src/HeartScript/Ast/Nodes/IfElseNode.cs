using System;

namespace HeartScript.Ast.Nodes
{
    public class IfElseNode : AstNode
    {
        public AstNode Condition { get; }
        public AstNode IfTrue { get; }
        public AstNode? IfFalse { get; }

        public IfElseNode(AstNode condition, AstNode ifTrue, AstNode? ifFalse) : base(typeof(void), AstType.Default)
        {
            if (condition.Type != typeof(bool))
                throw new ArgumentException($"{nameof(condition)} must be {typeof(bool)}");

            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }
    }

    public partial class AstNode
    {
        public static IfElseNode IfElse(AstNode condition, AstNode ifTrue, AstNode? ifFalse) => new IfElseNode(condition, ifTrue, ifFalse);
    }
}
