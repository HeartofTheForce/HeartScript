using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HeartScript.Ast.Nodes;
using HeartScript.Expressions;
using HeartScript.Parsing;
using HeartScript.Peg.Patterns;

namespace HeartScript.Ast
{
    public static class ExpressionBuilder
    {
        private delegate AstNode AstNodeBuilder(AstScope scope, ExpressionNode node);

        private static readonly Dictionary<string, AstNodeBuilder> s_nodeBuilders = new Dictionary<string, AstNodeBuilder>()
        {
            ["()"] = BuildRoundBracket,
            ["$"] = BuildStaticCall(typeof(Math)),
            ["u+"] = BuildPrefix(AstNode.UnaryPlus),
            ["u-"] = BuildPrefix(AstNode.Negate),
            ["~"] = BuildPrefix(AstNode.Not),
            ["!"] = BuildPostfix((node) => node),
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
            ["?:"] = BuildTernary,
            ["real"] = ParseReal,
            ["integral"] = ParseIntegral,
            ["boolean"] = ParseBoolean,
            ["identifier"] = ParseIdentifier,
        };

        public static AstNode Build(AstScope scope, ExpressionNode node)
        {
            if (node.Key != null && s_nodeBuilders.TryGetValue(node.Key, out var builder))
                return builder(scope, node);

            throw new ArgumentException($"{node.Key} does not have a matching builder");
        }

        static AstNode BuildRoundBracket(AstScope scope, ExpressionNode node)
        {
            var sequenceNode = (SequenceNode)node.MidNode;
            return Build(scope, (ExpressionNode)sequenceNode.Children[1]);
        }

        static AstNode ParseReal(AstScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (double.TryParse(valueNode.Value, out double value))
                return new ConstantNode(value);

            throw new ArgumentException(nameof(node));
        }

        static AstNode ParseIntegral(AstScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (int.TryParse(valueNode.Value, out int value))
                return new ConstantNode(value);

            throw new ArgumentException(nameof(node));
        }

        static AstNode ParseBoolean(AstScope scope, ExpressionNode node)
        {
            var choiceNode = (ChoiceNode)node.MidNode;
            var valueNode = (ValueNode)choiceNode.Node;
            if (bool.TryParse(valueNode.Value, out bool value))
                return new ConstantNode(value);

            throw new ArgumentException(nameof(node));
        }

        static AstNode ParseIdentifier(AstScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (scope.TryGetVariable(valueNode.Value, out var variable))
                return variable;

            throw new ArgumentException(nameof(node));
        }

        static AstNode BuildCall(AstScope scope, ExpressionNode callNode, AstNode? instance, Type type, BindingFlags bindingFlags)
        {
            if (callNode.LeftNode == null)
                throw new Exception($"{nameof(callNode.LeftNode)} cannot be null");

            if (callNode.LeftNode.Key != "identifier")
                throw new Exception($"{nameof(callNode.LeftNode)} is not identifier");

            var leftValueNode = (ValueNode)callNode.LeftNode.MidNode;
            string methodName = leftValueNode.Value;
            if (methodName == null)
                throw new Exception($"{nameof(methodName)} cannot be null");

            var parameterNodes = ParseNodeHelper.GetChildrenRecursive<ExpressionNode>(callNode.MidNode);
            var parameters = new AstNode[parameterNodes.Count];
            var parameterTypes = new Type[parameterNodes.Count];

            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = Build(scope, parameterNodes[i]);
                parameterTypes[i] = parameters[i].Type;
            }

            var methodInfo = type.GetMethod(methodName, bindingFlags, null, parameterTypes, null);
            if (methodInfo == null)
                throw new Exception($"{type.FullName} does not have an overload matching '{methodName}({string.Join(',', parameterTypes.Select(x => x.Name))})'");

            var expectedParameters = methodInfo.GetParameters();
            for (int i = 0; i < parameterNodes.Count; i++)
            {
                parameters[i] = AstBuilder.ConvertIfRequired(parameters[i], expectedParameters[i].ParameterType);
            }

            return AstNode.Call(instance, methodInfo, parameters);
        }

        static AstNodeBuilder BuildStaticCall(Type type)
        {
            return (scope, node) =>
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
                return BuildCall(scope, node, null, type, bindingFlags);
            };
        }

        static AstNodeBuilder BuildPrefix(Func<AstNode, AstNode> builder)
        {
            return (scope, node) =>
            {
                if (node.RightNode == null)
                    throw new Exception($"{nameof(node.RightNode)} cannot be null");

                var right = Build(scope, node.RightNode);

                return builder(right);
            };
        }

        static AstNodeBuilder BuildPostfix(Func<AstNode, AstNode> builder)
        {
            return (scope, node) =>
            {
                if (node.LeftNode == null)
                    throw new Exception($"{nameof(node.LeftNode)} cannot be null");

                var left = Build(scope, node.LeftNode);

                return builder(left);
            };
        }

        static AstNodeBuilder BuildBinary(Func<AstNode, AstNode, AstNode> builder)
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

        static AstNode BuildTernary(AstScope scope, ExpressionNode node)
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

        static bool IsReal(Type type) =>
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(decimal);

        static bool IsIntegral(Type type) =>
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
