using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HeartScript.Ast.Nodes;
using Heart.Parsing;
using Heart.Parsing.Patterns;

namespace HeartScript.Ast
{
    public static class ExpressionBuilder
    {
        private delegate AstNode AstNodeBuilder(SymbolScope scope, ExpressionNode node);

        private static readonly Dictionary<string, AstNodeBuilder> s_nodeBuilders = new Dictionary<string, AstNodeBuilder>()
        {
            ["()"] = BuildRoundBracket,
            ["$"] = BuildStaticCall(typeof(Math)),
            ["u+"] = BuildPrefix(AstNode.UnaryPlus),
            ["u-"] = BuildPrefix(AstNode.Negate),
            ["~"] = BuildPrefix(AstNode.Not),
            ["post++"] = BuildPostfix(AstNode.PostIncrement),
            ["post--"] = BuildPostfix(AstNode.PostDecrement),
            ["*"] = BuildBinary(AstNode.Multiply),
            ["/"] = BuildBinary(AstNode.Divide),
            ["+"] = BuildBinary(AstNode.Add),
            ["-"] = BuildBinary(AstNode.Subtract),
            ["<="] = BuildBinary(AstNode.LessThanOrEqual),
            [">="] = BuildBinary(AstNode.GreaterThanOrEqual),
            ["<"] = BuildBinary(AstNode.LessThan),
            [">"] = BuildBinary(AstNode.GreaterThan),
            ["=="] = BuildBinary(AstNode.Equal),
            ["!="] = BuildBinary(AstNode.NotEqual),
            ["&"] = BuildBinary(AstNode.And),
            ["^"] = BuildBinary(AstNode.ExclusiveOr),
            ["|"] = BuildBinary(AstNode.Or),
            ["="] = BuildBinary(AstNode.Assign),
            ["?:"] = BuildTernary,
            ["real"] = ParseReal,
            ["integral"] = ParseIntegral,
            ["boolean"] = ParseBoolean,
            ["identifier"] = ParseIdentifier,
        };

        public static AstNode Build(SymbolScope scope, ExpressionNode node)
        {
            if (node.Key != null && s_nodeBuilders.TryGetValue(node.Key, out var builder))
                return builder(scope, node);

            throw new ArgumentException($"{node.Key} has no matching builder");
        }

        private static AstNode BuildRoundBracket(SymbolScope scope, ExpressionNode node)
        {
            var sequenceNode = (SequenceNode)node.MidNode;
            return Build(scope, (ExpressionNode)sequenceNode.Children[1]);
        }

        private static AstNode ParseReal(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (double.TryParse(valueNode.Value, out double value))
                return new ConstantNode(value);

            throw new ArgumentException(nameof(node));
        }

        private static AstNode ParseIntegral(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (int.TryParse(valueNode.Value, out int value))
                return new ConstantNode(value);

            throw new ArgumentException(nameof(node));
        }

        private static AstNode ParseBoolean(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (bool.TryParse(valueNode.Value, out bool value))
                return new ConstantNode(value);

            throw new ArgumentException(nameof(node));
        }

        private static AstNode ParseIdentifier(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (scope.TryGetSymbol<AstNode>(valueNode.Value, out var symbol))
                return symbol.Value;

            throw new ArgumentException($"Missing symbol {valueNode.Value}");
        }

        private static MethodInfo ResolveMethodOverload(ExpressionNode? methodNode, Type type, BindingFlags bindingFlags, Type[] parameterTypes)
        {
            if (methodNode == null)
                throw new Exception($"{nameof(methodNode)} cannot be null");

            if (methodNode.Key != "identifier")
                throw new Exception($"{nameof(methodNode)} is not identifier");

            var valueNode = (ValueNode)methodNode.MidNode;
            string methodName = valueNode.Value;

            var methodInfo = type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
            if (methodInfo == null)
                throw new Exception($"{type.FullName} has no overload matching '{methodName}({string.Join(',', parameterTypes.Select(x => x.Name))})'");

            return methodInfo;
        }

        private static AstNode BuildCall(SymbolScope scope, ExpressionNode callNode, AstNode? instance, Type type, BindingFlags bindingFlags)
        {
            var parameterNodes = ParseNodeHelper.FindChildren<ExpressionNode>(callNode.MidNode);
            var parameters = new AstNode[parameterNodes.Count];
            var parameterTypes = new Type[parameterNodes.Count];

            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = Build(scope, parameterNodes[i]);
                parameterTypes[i] = parameters[i].Type;
            }

            var methodInfo = ResolveMethodOverload(callNode.LeftNode, type, bindingFlags, parameterTypes);
            var expectedParameters = methodInfo.GetParameters();
            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = AstBuilder.ConvertIfRequired(parameters[i], expectedParameters[i].ParameterType);
            }

            return AstNode.Call(instance, methodInfo, parameters);
        }

        private static AstNodeBuilder BuildStaticCall(Type type)
        {
            return (scope, node) =>
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
                return BuildCall(scope, node, null, type, bindingFlags);
            };
        }

        private static AstNodeBuilder BuildPrefix(Func<AstNode, AstNode> builder)
        {
            return (scope, node) =>
            {
                if (node.RightNode == null)
                    throw new Exception($"{nameof(node.RightNode)} cannot be null");

                var right = Build(scope, node.RightNode);

                return builder(right);
            };
        }

        private static AstNodeBuilder BuildPostfix(Func<AstNode, AstNode> builder)
        {
            return (scope, node) =>
            {
                if (node.LeftNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");

                var left = Build(scope, node.LeftNode);

                return builder(left);
            };
        }

        private static AstNodeBuilder BuildBinary(Func<AstNode, AstNode, AstNode> builder)
        {
            return (scope, node) =>
            {
                if (node.LeftNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");
                if (node.RightNode == null)
                    throw new Exception($"{nameof(node.RightNode)} cannot be null");

                var left = Build(scope, node.LeftNode);
                var right = Build(scope, node.RightNode);

                if (IsReal(left.Type) && IsIntegral(right.Type))
                    right = AstNode.Convert(right, left.Type);
                else if (IsIntegral(left.Type) && IsReal(right.Type))
                    left = AstNode.Convert(left, right.Type);

                return builder(left, right);
            };
        }

        private static AstNode BuildTernary(SymbolScope scope, ExpressionNode node)
        {
            if (node.LeftNode == null)
                throw new Exception($"{nameof(node.LeftNode)} cannot be null");
            if (node.RightNode == null)
                throw new Exception($"{nameof(node.RightNode)} cannot be null");

            var left = Build(scope, node.LeftNode);
            var right = Build(scope, node.RightNode);

            var sequenceNode = (SequenceNode)node.MidNode;
            var mid = Build(scope, (ExpressionNode)sequenceNode.Children[1]);

            if (IsIntegral(mid.Type) && IsReal(right.Type))
                mid = AstNode.Convert(mid, right.Type);
            else if (IsReal(mid.Type) && IsIntegral(right.Type))
                right = AstNode.Convert(right, mid.Type);

            return new ConditionalNode(left, mid, right);
        }

        private static bool IsReal(Type type) =>
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal);

        private static bool IsIntegral(Type type) =>
            type == typeof(sbyte) ||
            type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong);
    }
}
