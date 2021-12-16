using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Ast
{
    public static class MethodBuilder
    {
        private delegate void StatmentBuilder(SymbolScope scope, MethodInfoNode methodInfoNode, IParseNode parseNode);
        private static readonly Dictionary<string, StatmentBuilder> s_statementBuilders = new Dictionary<string, StatmentBuilder>()
        {
            ["declaration"] = BuildDeclaration,
            ["assignment"] = BuildAssignment,
            ["return"] = BuildReturn,
        };

        private delegate void BodyBuilder(SymbolScope scope, MethodInfoNode methodInfoNode, IParseNode parseNode);
        private static readonly Dictionary<string, BodyBuilder> s_bodyBuilders = new Dictionary<string, BodyBuilder>()
        {
            ["lambda"] = BuildLambdaBody,
            ["standard"] = BuildStandardBody,
        };

        private static void BuildBody(SymbolScope scope, MethodInfoNode methodInfoNode, LabelNode node)
        {
            if (node.Label != null && s_bodyBuilders.TryGetValue(node.Label, out var builder))
                builder(scope, methodInfoNode, node.Node);
            else
                throw new ArgumentException($"{node.Label} has no matching builder");
        }

        private static void BuildStatement(SymbolScope scope, MethodInfoNode methodInfoNode, LabelNode node)
        {
            if (node.Label != null && s_statementBuilders.TryGetValue(node.Label, out var builder))
                builder(scope, methodInfoNode, node.Node);
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

            var parameters = new Type[parameterValues.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = GetType(parameterValues[i].Item1);
                string paramName = GetName(parameterValues[i].Item2);

                var parameterNode = new ParameterNode(i, paramType);

                parameters[i] = parameterNode.Type;
                methodScope.DeclareSymbol(paramName, new Symbol<AstNode>(false, parameterNode));
            }

            var methodInfoNode = new MethodInfoNode(methodName, returnType, parameters);
            var methodBodyChoiceNode = (ChoiceNode)methodSequence.Children[5];
            var methodBodyLabelNode = (LabelNode)methodBodyChoiceNode.Node;
            BuildBody(methodScope, methodInfoNode, methodBodyLabelNode);

            return methodInfoNode;
        }

        private static void BuildLambdaBody(SymbolScope scope, MethodInfoNode methodInfoNode, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var expressionNode = (ExpressionNode)sequenceNode.Children[1];
            var expression = AstBuilder.ConvertIfRequired(ExpressionBuilder.Build(scope, expressionNode), methodInfoNode.ReturnType);
            methodInfoNode.Statements.Add(AstNode.Return(expression));
        }

        private static void BuildStandardBody(SymbolScope scope, MethodInfoNode methodInfoNode, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var quantifierNode = (QuantifierNode)sequenceNode.Children[1];

            foreach (var child in quantifierNode.Children)
            {
                var choiceNode = (ChoiceNode)child;
                var labelNode = (LabelNode)choiceNode.Node;
                BuildStatement(scope, methodInfoNode, labelNode);
            }
        }

        private static void BuildDeclaration(SymbolScope scope, MethodInfoNode methodInfoNode, IParseNode node)
        {
            var declarationSequence = (SequenceNode)node;
            var type = GetType(declarationSequence.Children[0]);
            string name = GetName(declarationSequence.Children[1]);

            var expressionNode = (ExpressionNode)declarationSequence.Children[2];
            var expression = ExpressionBuilder.Build(scope, expressionNode);

            // scope.DeclareSymbol(
        }

        private static void BuildAssignment(SymbolScope scope, MethodInfoNode methodInfoNode, IParseNode node)
        {
            throw new NotImplementedException();
        }

        private static void BuildReturn(SymbolScope scope, MethodInfoNode methodInfoNode, IParseNode node)
        {
            var returnSequence = (SequenceNode)node;
            var expressionNode = returnSequence.Children[1];
            var expression = AstBuilder.ConvertIfRequired(AstBuilder.Build(scope, expressionNode), methodInfoNode.ReturnType);

            methodInfoNode.Statements.Add(AstNode.Return(expression));
        }

        private static Type GetType(IParseNode typeNode)
        {
            var choiceNode = (ChoiceNode)typeNode;
            var valueNode = (ValueNode)choiceNode.Node;
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
    }
}
