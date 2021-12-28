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
        private delegate void StatmentBuilder(SymbolScope scope, MethodInfoBuilder builder, IParseNode parseNode);
        private static readonly Dictionary<string, StatmentBuilder> s_statementBuilders = new Dictionary<string, StatmentBuilder>()
        {
            ["lambda"] = BuildLambdaBody,
            ["block_statement"] = BuildBlockBody,
            ["declaration"] = BuildDeclaration,
            ["declaration_statement"] = BuildSemicolonStatement,
            ["expr"] = BuildExpressionStatement,
            ["expr_statement"] = BuildSemicolonStatement,
            ["return_statement"] = BuildReturn,
        };

        private static void BuildStatement(SymbolScope scope, MethodInfoBuilder builder, LabelNode node)
        {
            if (node.Label != null && s_statementBuilders.TryGetValue(node.Label, out var statmentBuilder))
                statmentBuilder(scope, builder, node.Node);
            else
                throw new ArgumentException($"{node.Label} has no matching builder");
        }

        public static MethodBodyNode BuildMethodBody(SymbolScope scope, MethodInfo methodInfo, IParseNode node)
        {
            var methodInfoBuilder = new MethodInfoBuilder(methodInfo.ReturnType);

            var labelNode = (LabelNode)node;
            BuildStatement(scope, methodInfoBuilder, labelNode);

            return new MethodBodyNode(
                methodInfoBuilder.Variables.ToArray(),
                AstNode.Block(methodInfoBuilder.Statements.ToArray())
            );
        }

        private static void BuildLambdaBody(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var expressionNode = (ExpressionNode)sequenceNode.Children[1];
            var expression = ExpressionBuilder.Build(scope, expressionNode);

            var returnNode = AstNode.Return(AstBuilder.ConvertIfRequired(expression, builder.ReturnType));
            builder.Statements.Add(returnNode);
        }

        private static void BuildBlockBody(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var quantifierNode = (QuantifierNode)sequenceNode.Children[1];

            var blockScope = new SymbolScope(scope);
            foreach (var child in quantifierNode.Children)
            {
                var labelNode = (LabelNode)child;
                BuildStatement(blockScope, builder, labelNode);
            }
        }

        private static void BuildSemicolonStatement(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var statementSequence = (SequenceNode)node;
            var labelNode = (LabelNode)statementSequence.Children[0];
            BuildStatement(scope, builder, labelNode);
        }

        private static void BuildDeclaration(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var declarationSequence = (SequenceNode)node;

            var type = GetType(declarationSequence.Children[0]);
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
                builder.Statements.Add(assignNode);
            }
        }

        private static void BuildExpressionStatement(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var expressionNode = (ExpressionNode)node;
            var expression = ExpressionBuilder.Build(scope, expressionNode);

            builder.Statements.Add(expression);
        }

        private static void BuildReturn(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var returnSequence = (SequenceNode)node;
            var expressionNode = (ExpressionNode)returnSequence.Children[1];
            var expression = ExpressionBuilder.Build(scope, expressionNode);

            var returnNode = AstNode.Return(AstBuilder.ConvertIfRequired(expression, builder.ReturnType));
            builder.Statements.Add(returnNode);
        }

        private static Type GetType(IParseNode typeNode)
        {
            var valueNode = (ValueNode)typeNode;
            switch (valueNode.Value)
            {
                case "int": return typeof(int);
                case "double": return typeof(double);
                case "bool": return typeof(bool);
                default: throw new NotImplementedException();
            }
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
            public List<AstNode> Statements { get; }

            public MethodInfoBuilder(Type returnType)
            {
                ReturnType = returnType;
                Variables = new List<VariableNode>();
                Statements = new List<AstNode>();
            }
        }
    }
}
