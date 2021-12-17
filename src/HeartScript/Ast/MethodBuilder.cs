using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Ast
{
    public static class MethodBuilder
    {
        private delegate void StatmentBuilder(SymbolScope scope, MethodInfoBuilder builder, IParseNode parseNode);
        private static readonly Dictionary<string, StatmentBuilder> s_statementBuilders = new Dictionary<string, StatmentBuilder>()
        {
            ["lambda"] = BuildLambdaBody,
            ["block"] = BuildBlockBody,
            ["declaration"] = BuildDeclaration,
            ["statement_expr"] = BuildStatementExpression,
            ["return"] = BuildReturn,
        };

        private static void BuildStatement(SymbolScope scope, MethodInfoBuilder builder, LabelNode node)
        {
            if (node.Label != null && s_statementBuilders.TryGetValue(node.Label, out var statmentBuilder))
                statmentBuilder(scope, builder, node.Node);
            else
                throw new ArgumentException($"{node.Label} has no matching builder");
        }

        public static AstNode BuildMethod(SymbolScope scope, IParseNode node)
        {
            var methodSequence = (SequenceNode)node;

            string methodName = GetName(methodSequence.Children[1]);
            var returnType = GetType(methodSequence.Children[0]);

            var parameterValues = new List<(IParseNode, IParseNode)>();
            var parameterMatch = (QuantifierNode)methodSequence.Children[3];
            if (parameterMatch.Children.Count > 0)
            {
                var head = (SequenceNode)parameterMatch.Children[0];
                var paramTypeNode = head.Children[0];
                var paramNameNode = head.Children[1];
                parameterValues.Add((paramTypeNode, paramNameNode));

                var tail = (QuantifierNode)head.Children[2];
                foreach (var tailChild in tail.Children)
                {
                    var tailSequence = (SequenceNode)tailChild;
                    paramTypeNode = tailSequence.Children[1];
                    paramNameNode = tailSequence.Children[2];
                    parameterValues.Add((paramTypeNode, paramNameNode));
                }
            }

            var methodScope = new SymbolScope(scope);

            var methodInfoBuilder = new MethodInfoBuilder(methodName, returnType);
            for (int i = 0; i < parameterValues.Count; i++)
            {
                var paramType = GetType(parameterValues[i].Item1);
                string paramName = GetName(parameterValues[i].Item2);

                var parameterNode = AstNode.Parameter(i, paramType);
                methodInfoBuilder.ParameterTypes.Add(parameterNode.Type);

                var symbol = new Symbol<AstNode>(true, parameterNode);
                methodScope.DeclareSymbol(paramName, symbol);
            }

            var methodBodyLabelNode = (LabelNode)methodSequence.Children[5];
            BuildStatement(methodScope, methodInfoBuilder, methodBodyLabelNode);

            return new MethodInfoNode(
                methodInfoBuilder.Name,
                methodInfoBuilder.ReturnType,
                methodInfoBuilder.ParameterTypes.ToArray(),
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

        private static readonly ICollection<AstType> s_validStatmentExpressions = new HashSet<AstType>()
        {
            AstType.Assign,
        };

        private static void BuildStatementExpression(SymbolScope scope, MethodInfoBuilder builder, IParseNode node)
        {
            var statementSequence = (SequenceNode)node;
            var expressionNode = (ExpressionNode)statementSequence.Children[0];

            var expression = ExpressionBuilder.Build(scope, expressionNode);
            if (!s_validStatmentExpressions.Contains(expression.NodeType))
                throw new ArgumentException("Invalid statement expression");

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
            public string Name { get; }
            public Type ReturnType { get; }
            public List<Type> ParameterTypes { get; }
            public List<VariableNode> Variables { get; }
            public List<AstNode> Statements { get; }

            public MethodInfoBuilder(string name, Type returnType)
            {
                Name = name;
                ReturnType = returnType;
                ParameterTypes = new List<Type>();
                Variables = new List<VariableNode>();
                Statements = new List<AstNode>();
            }
        }
    }
}
