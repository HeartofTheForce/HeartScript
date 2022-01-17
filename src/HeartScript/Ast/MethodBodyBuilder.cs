using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using Heart.Parsing;
using Heart.Parsing.Patterns;
using System.Reflection;

namespace HeartScript.Ast
{
    public static class MethodBodyBuilder
    {
        private delegate AstNode? StatmentBuilder(SymbolScope scope, MethodInfoBuilder builder, IParseNode parseNode);
        private static readonly Dictionary<string, StatmentBuilder> s_statementBuilders = new Dictionary<string, StatmentBuilder>()
        {
            ["lambda"] = BuildLambdaBody,
            ["block_statement"] = BuildBlockBody,
            ["declaration"] = BuildDeclaration,
            ["declaration_statement"] = BuildSemicolonStatement,
            ["return_statement"] = BuildReturn,
            ["for_statement"] = BuildForStatement,
            ["while_statement"] = BuildWhileStatement,
            ["do_statement"] = BuildDoStatement,
            ["expr"] = BuildExpression,
            ["expr_statement"] = BuildSemicolonStatement,
        };

        private static AstNode? BuildStatement(SymbolScope scope, MethodInfoBuilder builder, LabelNode node)
        {
            if (node.Label != null && s_statementBuilders.TryGetValue(node.Label, out var statmentBuilder))
                return statmentBuilder(scope, builder, node.Node);
            else
                throw new ArgumentException($"{node.Label} has no matching builder");
        }

        public static MethodBodyNode BuildMethodBody(SymbolScope scope, MethodInfo methodInfo, IParseNode node)
        {
            var methodInfoBuilder = new MethodInfoBuilder(methodInfo.ReturnType);

            var labelNode = (LabelNode)node;
            var blockNode = BuildStatement(scope, methodInfoBuilder, labelNode) as BlockNode;

            return new MethodBodyNode(
                methodInfoBuilder.Variables.ToArray(),
                blockNode ?? throw new ArgumentException(nameof(blockNode))
            );
        }

        private static AstNode? BuildLambdaBody(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var expressionNode = (ExpressionNode)sequenceNode.Children[1];
            var expression = ExpressionBuilder.Build(scope, expressionNode);

            var returnNode = AstNode.Return(AstBuilder.ConvertIfRequired(expression, builder.ReturnType));
            return new BlockNode(new AstNode[] { returnNode });
        }

        private static BlockNode? BuildBlockBody(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var quantifierNode = (QuantifierNode)sequenceNode.Children[1];

            var blockScope = new SymbolScope(scope);
            var blockStatements = new List<AstNode>();
            foreach (var child in quantifierNode.Children)
            {
                var labelNode = (LabelNode)child;
                var statement = BuildStatement(blockScope, builder, labelNode);
                if (statement != null)
                    blockStatements.Add(statement);
            }

            return new BlockNode(blockStatements.ToArray());
        }

        private static AstNode? BuildSemicolonStatement(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var statementSequence = (SequenceNode)node;
            var labelNode = (LabelNode)statementSequence.Children[0];
            return BuildStatement(scope, builder, labelNode);
        }

        private static AstNode? BuildDeclaration(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var declarationSequence = (SequenceNode)node;

            var type = TypeHelper.ResolveTypeNode(declarationSequence.Children[0]);
            string name = GetName(declarationSequence.Children[1]);

            var variableNode = AstNode.Variable(builder.Variables.Count, type);
            builder.Variables.Add(variableNode);

            var symbol = new Symbol<AstNode>(true, variableNode);
            scope.DeclareSymbol(name, symbol);

            var optionalAssignmentNode = (QuantifierNode)declarationSequence.Children[2];
            if (optionalAssignmentNode.Children.Count > 0)
            {
                var assignmentSequence = (SequenceNode)optionalAssignmentNode.Children[0];
                var expressionNode = (ExpressionNode)assignmentSequence.Children[1];
                var expression = ExpressionBuilder.Build(scope, expressionNode);

                var assignNode = AstNode.Assign(variableNode, AstBuilder.ConvertIfRequired(expression, variableNode.Type));
                return assignNode;
            }

            return null;
        }

        private static AstNode? BuildReturn(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var returnSequence = (SequenceNode)node;
            var expressionNode = (ExpressionNode)returnSequence.Children[1];
            var expression = ExpressionBuilder.Build(scope, expressionNode);

            var returnNode = AstNode.Return(AstBuilder.ConvertIfRequired(expression, builder.ReturnType));
            return returnNode;
        }

        private static AstNode? BuildForStatement(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var forSequence = (SequenceNode)node;

            var initializerNode = (QuantifierNode)forSequence.Children[2];
            AstNode? initialize = null;
            if (initializerNode.Children.Count > 0)
            {
                initialize = BuildStatement(scope, builder, (LabelNode)initializerNode.Children[0]);
            }

            var conditionNode = (QuantifierNode)forSequence.Children[4];
            AstNode? condition = null;
            if (conditionNode.Children.Count > 0)
            {
                condition = ExpressionBuilder.Build(scope, (ExpressionNode)conditionNode.Children[0]);
            }

            var stepNode = (QuantifierNode)forSequence.Children[6];
            AstNode? step = null;
            if (stepNode.Children.Count > 0)
            {
                step = ExpressionBuilder.Build(scope, (ExpressionNode)stepNode.Children[0]);
            }

            var bodyNode = (LabelNode)forSequence.Children[8];
            var body = BuildStatement(scope, builder, bodyNode);
            if (body == null)
                throw new ArgumentException(nameof(body));

            var loopNode = AstNode.Loop(initialize, step, condition, body, false);
            return loopNode;
        }

        private static AstNode? BuildWhileStatement(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var whileSequence = (SequenceNode)node;

            var conditionNode = (ExpressionNode)whileSequence.Children[2];
            var condition = ExpressionBuilder.Build(scope, conditionNode);

            var bodyNode = (LabelNode)whileSequence.Children[4];
            var body = BuildStatement(scope, builder, bodyNode);
            if (body == null)
                throw new ArgumentException(nameof(body));

            var loopNode = AstNode.Loop(null, null, condition, body, false);
            return loopNode;
        }

        private static AstNode? BuildDoStatement(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var doWhileSequence = (SequenceNode)node;

            var bodyNode = (LabelNode)doWhileSequence.Children[1];
            var body = BuildStatement(scope, builder, bodyNode);
            if (body == null)
                throw new ArgumentException(nameof(body));

            var conditionNode = (ExpressionNode)doWhileSequence.Children[4];
            var condition = ExpressionBuilder.Build(scope, conditionNode);

            var loopNode = AstNode.Loop(null, null, condition, body, true);
            return loopNode;
        }

        private static AstNode? BuildExpression(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var expressionNode = (ExpressionNode)node;
            var expression = ExpressionBuilder.Build(scope, expressionNode);

            return expression;
        }

        private static string GetName(IParseNode nameNode)
        {
            var sequenceNode = (SequenceNode)nameNode;
            var valueNode = (ValueNode)sequenceNode.Children[1];
            return valueNode.Value;
        }

        private class MethodInfoBuilder
        {
            public Type ReturnType { get; }
            public List<VariableNode> Variables { get; }

            public MethodInfoBuilder(Type returnType)
            {
                ReturnType = returnType;
                Variables = new List<VariableNode>();
            }
        }
    }
}
