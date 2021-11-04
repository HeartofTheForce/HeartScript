using System;
using HeartScript.Ast.Nodes;
using HeartScript.Expressions;
using HeartScript.Parsing;
using HeartScript.Peg.Patterns;
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
                case LabelNode labelNode: return LabelBuilder.Build(scope, labelNode);
                default: throw new NotImplementedException();
            }
        }

        public static AstNode ConvertIfRequired(AstNode expression, Type expectedType)
        {
            if (expression.Type != expectedType)
                return AstNode.Convert(expression, expectedType);

            return expression;
        }
    }
}
