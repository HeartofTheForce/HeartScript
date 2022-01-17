using System;

namespace HeartScript.Ast.Nodes
{
    public class LoopNode : AstNode
    {
        public AstNode? Initialize { get; }
        public AstNode? Step { get; }
        public AstNode? Condition { get; }
        public AstNode Body { get; }
        public bool RunAtLeastOnce { get; }

        public LoopNode(
            AstNode? initialize,
            AstNode? step,
            AstNode? condition,
            AstNode body,
            bool runAtLeastOnce) : base(typeof(void), AstType.Default)
        {
            if (condition != null && condition.Type != typeof(bool))
                throw new ArgumentException($"{nameof(condition)} must be null or {typeof(bool)}");

            Initialize = initialize;
            Step = step;
            Condition = condition;
            RunAtLeastOnce = runAtLeastOnce;
            Body = body;
        }
    }

    public partial class AstNode
    {
        public static LoopNode Loop(
            AstNode? initialize,
            AstNode? step,
            AstNode? condition,
            AstNode body,
            bool runAtLeastOnce) => new LoopNode(initialize, step, condition, body, runAtLeastOnce);
    }
}
