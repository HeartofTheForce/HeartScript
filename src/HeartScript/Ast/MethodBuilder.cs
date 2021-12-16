using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Ast
{
    public static class MethodBuilder
    {
        private delegate AstNode StatmentBuilder(SymbolScope scope, Type returnType, IParseNode parseNode);
        private static readonly Dictionary<string, StatmentBuilder> s_statementBuilders = new Dictionary<string, StatmentBuilder>()
        {
            ["declaration"] = BuildDeclaration,
            ["assignment"] = BuildAssignment,
            ["return"] = BuildReturn,
        };

        private delegate BlockNode BodyBuilder(SymbolScope scope, Type returnType, IParseNode parseNode);
        private static readonly Dictionary<string, BodyBuilder> s_bodyBuilders = new Dictionary<string, BodyBuilder>()
        {
            ["lambda"] = BuildLambdaBody,
            ["standard"] = BuildStandardBody,
        };

        private static BlockNode BuildBody(SymbolScope scope, Type returnType, LabelNode node)
        {
            if (node.Label != null && s_bodyBuilders.TryGetValue(node.Label, out var builder))
                return builder(scope, returnType, node.Node);

            throw new ArgumentException($"{node.Label} has no matching builder");
        }

        private static AstNode BuildStatement(SymbolScope scope, Type returnType, LabelNode node)
        {
            if (node.Label != null && s_statementBuilders.TryGetValue(node.Label, out var builder))
                return builder(scope, returnType, node.Node);

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

            var body = BuildMethodBody(methodScope, returnType, methodSequence.Children[5]);

            return new MethodInfoNode(methodName, parameters, body);
        }

        private static BlockNode BuildMethodBody(SymbolScope scope, Type returnType, IParseNode body)
        {
            var choiceNode = (ChoiceNode)body;
            var labelNode = (LabelNode)choiceNode.Node;

            return BuildBody(scope, returnType, labelNode);
        }

        private static BlockNode BuildLambdaBody(SymbolScope scope, Type returnType, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var expressionNode = (ExpressionNode)sequenceNode.Children[1];
            var expression = AstBuilder.ConvertIfRequired(ExpressionBuilder.Build(scope, expressionNode), returnType);
            var statements = new AstNode[]
            {
                AstNode.Return(expression),
            };

            return AstNode.Block(statements, returnType);
        }

        private static BlockNode BuildStandardBody(SymbolScope scope, Type returnType, IParseNode node)
        {
            var sequenceNode = (SequenceNode)node;
            var quantifierNode = (QuantifierNode)sequenceNode.Children[1];

            var statements = new List<AstNode>();
            foreach (var child in quantifierNode.Children)
            {
                var choiceNode = (ChoiceNode)child;
                var labelNode = (LabelNode)choiceNode.Node;
                statements.Add(BuildStatement(scope, returnType, labelNode));
            }

            return AstNode.Block(statements.ToArray(), returnType);
        }

        private static AstNode BuildDeclaration(SymbolScope scope, Type returnType, IParseNode node)
        {
            throw new NotImplementedException();
        }

        private static AstNode BuildAssignment(SymbolScope scope, Type returnType, IParseNode node)
        {
            throw new NotImplementedException();
        }

        private static AstNode BuildReturn(SymbolScope scope, Type returnType, IParseNode node)
        {
            var returnSequence = (SequenceNode)node;
            var expressionNode = returnSequence.Children[1];
            var expression = AstBuilder.ConvertIfRequired(AstBuilder.Build(scope, expressionNode), returnType);

            return AstNode.Return(expression);
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
