using System;
using HeartScript.Ast.Nodes;
using HeartScript.Expressions;
using HeartScript.Parsing;
#pragma warning disable IDE0066

namespace HeartScript.Ast
{
    public static class AstBuilder
    {
        public static AstNode Build(AstScope scope, IParseNode node)
        {
            switch (node)
            {
                case ExpressionNode expressionNode: return ExpressionBuilder.Build(scope, expressionNode);
                default: throw new NotImplementedException();
            }
        }
    }
}
