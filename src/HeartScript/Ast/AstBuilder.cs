using System;
using HeartScript.Ast.Nodes;

namespace HeartScript.Ast
{
    public static class AstBuilder
    {
        public static AstNode ConvertIfRequired(AstNode expression, Type expectedType)
        {
            if (expression.Type != expectedType)
                return AstNode.Convert(expression, expectedType);

            return expression;
        }
    }
}
