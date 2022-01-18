using System;

namespace HeartScript.Ast.Nodes
{
    public class ConditionalNode : AstNode
    {
        public AstNode Condition { get; }
        public AstNode IfTrue { get; }
        public AstNode IfFalse { get; }

        public ConditionalNode(AstNode condition, AstNode ifTrue, AstNode ifFalse) : base(ifTrue.Type, AstType.Default)
        {
            if (condition.Type != typeof(bool))
                throw new ArgumentException($"{nameof(condition)} must be {typeof(bool)}");

            if (ifTrue.Type != ifFalse.Type)
                throw new ArgumentException($"{nameof(ifTrue)} and {nameof(ifFalse)} must be the same Type");

            Condition = condition;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }
    }

    public partial class AstNode
    {
        public static ConditionalNode Conditional(AstNode condition, AstNode ifTrue, AstNode ifFalse) => new ConditionalNode(condition, ifTrue, ifFalse);
    }
}
