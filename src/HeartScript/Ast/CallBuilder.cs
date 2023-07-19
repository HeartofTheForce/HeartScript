using System;
using System.Linq;
using System.Reflection;
using HeartScript.Ast.Nodes;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Ast
{
    public static class CallBuilder
    {
        public static AstNode BuildStaticCall(SymbolScope scope, ExpressionNode callNode)
        {
            var parameterNodes = callNode.MidNode.FindChildren<ExpressionNode>();
            var parameters = new AstNode[parameterNodes.Count];
            var parameterTypes = new Type[parameterNodes.Count];

            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = ExpressionBuilder.Build(scope, parameterNodes[i]);
                parameterTypes[i] = parameters[i].Type;
            }

            var scripMethod = ResolveMethodOverload(scope, callNode.LeftNode, parameterTypes);
            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = AstBuilder.ConvertIfRequired(parameters[i], scripMethod.ParameterTypes[i]);
            }

            return AstNode.Call(null, scripMethod, parameters);
        }

        private static ScriptType ResolveMemberAccessType(SymbolScope scope, ExpressionNode? typeNode)
        {
            if (typeNode == null)
                throw new Exception($"{nameof(typeNode)} cannot be null");

            if (typeNode.Key != "identifier")
                throw new Exception($"{nameof(typeNode)} is not identifier");

            var typeNameNode = (ValueNode)typeNode.MidNode;
            string typeName = typeNameNode.Value;

            if (scope.TryGetSymbol<ScriptType>(typeName, out var symbol))
                return symbol.Value;

            throw new ArgumentException($"Missing {nameof(Type)} symbol, {typeName}");
        }

        private static ScriptType ResolveCurrentType(SymbolScope scope)
        {
            if (scope.TryGetSymbol<ScriptType>(ScriptType.CurrentTypeKey, out var symbol))
                return symbol.Value;

            throw new ArgumentException($"Missing {nameof(Type)} symbol, {ScriptType.CurrentTypeKey}");
        }

        private static string ResolveCallName(ExpressionNode? methodNameNode)
        {
            if (methodNameNode == null)
                throw new Exception($"{nameof(methodNameNode)} cannot be null");

            if (methodNameNode.Key != "identifier")
                throw new Exception($"{nameof(methodNameNode)} is not identifier");

            var valueNode = (ValueNode)methodNameNode.MidNode;
            return valueNode.Value;
        }

        private static ScriptMethod ResolveMethodOverload(SymbolScope scope, ExpressionNode? node, Type[] parameterTypes)
        {
            if (node == null)
                throw new Exception($"{nameof(node)} cannot be null");

            ScriptType type;
            string methodName;
            switch (node.Key)
            {
                case ".":
                    {
                        type = ResolveMemberAccessType(scope, node.LeftNode);
                        methodName = ResolveCallName(node.RightNode);
                    }
                    break;
                default:
                    {
                        type = ResolveCurrentType(scope);
                        methodName = ResolveCallName(node);
                    }
                    break;
            }

            var bindingFlags = BindingFlags.Public | BindingFlags.Static;
            var scripMethod = type.GetMethod(methodName, bindingFlags, parameterTypes)
                ?? throw new Exception($"{type.Name} has no overload matching '{methodName}({string.Join(',', parameterTypes.Select(x => x.Name))})'");

            return scripMethod;
        }
    }
}
