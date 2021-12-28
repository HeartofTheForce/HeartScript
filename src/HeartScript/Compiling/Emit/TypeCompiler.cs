using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Heart.Parsing;
using Heart.Parsing.Patterns;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public static class TypeCompiler
    {
        public static Type Compile(SymbolScope scope, TypeBuilder typeBuilder, IParseNode node)
        {
            var quantifierNode = (QuantifierNode)node;
            var methodSignatureResults = new List<(System.Reflection.Emit.MethodBuilder, SymbolScope, IParseNode)>();
            foreach (var child in quantifierNode.Children)
            {
                var labelNode = (LabelNode)child;
                var methodSignatureResult = EmitMethodSignature(scope, typeBuilder, labelNode.Node);
                methodSignatureResults.Add(methodSignatureResult);
            }

            foreach (var (methodBuilder, methodScope, methodBodyNode) in methodSignatureResults)
            {
                var methodBodyAst = Ast.MethodBuilder.BuildMethodBody(methodScope, methodBuilder, methodBodyNode);
                MethodCompiler.EmitMethodBody(methodBuilder, methodBodyAst);
            }

            var loadedType = typeBuilder.CreateType();

            return loadedType;
        }

        public static (System.Reflection.Emit.MethodBuilder, SymbolScope, IParseNode) EmitMethodSignature(SymbolScope scope, TypeBuilder typeBuilder, IParseNode node)
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
            var parameterTypes = new Type[parameterValues.Count];
            for (int i = 0; i < parameterValues.Count; i++)
            {
                var paramType = GetType(parameterValues[i].Item1);
                string paramName = GetName(parameterValues[i].Item2);

                var parameterNode = AstNode.Parameter(i, paramType);
                parameterTypes[i] = parameterNode.Type;

                var symbol = new Symbol<AstNode>(true, parameterNode);
                methodScope.DeclareSymbol(paramName, symbol);
            }

            var methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, returnType, parameterTypes);
            var methodBodyNode = methodSequence.Children[5];
            return (methodBuilder, methodScope, methodBodyNode);
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
    }
}
