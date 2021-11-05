using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using HeartScript.Parsing;
using HeartScript.Peg.Patterns;
#pragma warning disable IDE0066

namespace HeartScript.Ast
{
    public static class TypeBuilder
    {
        private delegate AstNode AstNodeBuilder(AstScope scope, LabelNode node);

        private static readonly Dictionary<string, AstNodeBuilder> s_nodeBuilders = new Dictionary<string, AstNodeBuilder>()
        {
            ["method"] = BuildMethod,
        };

        public static AstNode Build(AstScope scope, LabelNode node)
        {
            if (node.Label != null && s_nodeBuilders.TryGetValue(node.Label, out var builder))
                return builder(scope, node);

            throw new ArgumentException($"{node.Label} does not have a matching builder");
        }

        static AstNode BuildMethod(AstScope scope, LabelNode node)
        {
            var methodSequence = (SequenceNode)node.Node;

            string methodName = ((ValueNode)methodSequence.Children[1]).Value;
            var returnType = GetType(methodSequence.Children[0]);

            var parameterValues = new List<(ChoiceNode, ValueNode)>();
            var parameterMatch = (QuantifierNode)methodSequence.Children[3];
            if (parameterMatch.Children.Count > 0)
            {
                var head = (SequenceNode)parameterMatch.Children[0];
                var paramTypeNode = (ChoiceNode)head.Children[0];
                var paramNameNode = (ValueNode)head.Children[1];
                parameterValues.Add((paramTypeNode, paramNameNode));

                var tail = (QuantifierNode)head.Children[2];
                foreach (var tailChild in tail.Children)
                {
                    var tailSequence = (SequenceNode)tailChild;
                    paramTypeNode = (ChoiceNode)tailSequence.Children[1];
                    paramNameNode = (ValueNode)tailSequence.Children[2];
                    parameterValues.Add((paramTypeNode, paramNameNode));
                }
            }

            var methodScope = new AstScope(scope);

            var parameters = new Type[parameterValues.Count];
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = GetType(parameterValues[i].Item1);
                string paramName = parameterValues[i].Item2.Value;

                var parameterNode = new ParameterNode(i, paramType);

                parameters[i] = parameterNode.Type;
                methodScope.SetMember(paramName, parameterNode, false);
            }

            var body = BuildMethodBody(methodScope, returnType, methodSequence.Children[5]);

            return new MethodInfoNode(methodName, parameters, body);
        }

        static BlockNode BuildMethodBody(AstScope scope, Type returnType, IParseNode body)
        {
            var choiceNode = (ChoiceNode)body;
            var labelNode = (LabelNode)choiceNode.Node;
            var sequenceNode = (SequenceNode)labelNode.Node;

            var statements = new List<AstNode>();
            switch (labelNode.Label)
            {
                case "standard":
                    {
                        var quantifierNode = (QuantifierNode)sequenceNode.Children[1];
                        foreach (var child in quantifierNode.Children)
                        {
                            var statementSequence = (SequenceNode)child;
                            var returnSequence = (SequenceNode)statementSequence.Children[0];
                            var expressionNode = returnSequence.Children[1];
                            var expression = AstBuilder.ConvertIfRequired(AstBuilder.Build(scope, expressionNode), returnType);
                            statements.Add(AstNode.Return(expression));
                        }
                    }
                    break;
                case "lambda":
                    {
                        var expressionNode = sequenceNode.Children[1];
                        var expression = AstBuilder.ConvertIfRequired(AstBuilder.Build(scope, expressionNode), returnType);
                        statements.Add(AstNode.Return(expression));
                    }
                    break;
                default: throw new NotImplementedException();
            }

            return AstNode.Block(statements.ToArray(), returnType);
        }

        static Type GetType(IParseNode typeNode)
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
    }
}
