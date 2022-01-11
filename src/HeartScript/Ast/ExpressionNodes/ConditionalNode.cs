using System;

namespace HeartScript.Ast.Nodes
{
    public class ConditionalNode : AstNode
    {
        public AstNode Test { get; }
        public AstNode IfTrue { get; }
        public AstNode IfFalse { get; }

        public ConditionalNode(AstNode test, AstNode ifTrue, AstNode ifFalse) : base(ifTrue.Type, AstType.Default)
        {
            if (test.Type != typeof(bool))
                throw new ArgumentException($"{nameof(test)} must be {typeof(bool)}");

            if (ifTrue.Type != ifFalse.Type)
                throw new ArgumentException($"{nameof(ifTrue)} and {nameof(ifFalse)} must be the same Type");

            Test = test;
            IfTrue = ifTrue;
            IfFalse = ifFalse;
        }
    }
}
