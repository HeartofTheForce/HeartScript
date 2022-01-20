using System;
using System.Collections.Generic;
using HeartScript.Ast.Nodes;
using Heart.Parsing.Patterns;

namespace HeartScript.Ast
{
    public static class ExpressionBuilder
    {
        private delegate AstNode AstNodeBuilder(SymbolScope scope, ExpressionNode node);

        private static readonly Dictionary<string, AstNodeBuilder> s_nodeBuilders = new Dictionary<string, AstNodeBuilder>()
        {
            ["()"] = BuildRoundBracket,
            ["new[]"] = BuildArrayConstructor,
            ["len"] = BuildLen,
            ["u+"] = BuildPrefix(AstNode.UnaryPlus),
            ["u-"] = BuildPrefix(AstNode.Negate),
            ["~"] = BuildPrefix(AstNode.Not),
            ["real"] = ParseReal,
            ["integral"] = ParseIntegral,
            ["boolean"] = ParseBoolean,
            ["identifier"] = ParseIdentifier,
            ["."] = MemberAccessBuilder.BuildMemberAccess,
            ["$"] = CallBuilder.BuildStaticCall,
            ["[]"] = BuildArrayIndex,
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

        private static AstNode BuildArrayConstructor(SymbolScope scope, ExpressionNode node)
        {
            var sequenceNode = (SequenceNode)node.MidNode;
            var type = TypeHelper.ResolveTypeNode(sequenceNode.Children[1]).MakeArrayType();
            var length = Build(scope, (ExpressionNode)sequenceNode.Children[3]);

            return AstNode.ArrayConstructor(type, length);
        }

        private static AstNode BuildArrayIndex(SymbolScope scope, ExpressionNode node)
        {
            if (node.LeftNode == null)
                throw new Exception($"{nameof(node.LeftNode)} cannot be null");

            var array = Build(scope, node.LeftNode);

            var sequenceNode = (SequenceNode)node.MidNode;
            var index = Build(scope, (ExpressionNode)sequenceNode.Children[1]);

            return AstNode.ArrayIndex(array, index);
        }

        private static AstNode BuildLen(SymbolScope scope, ExpressionNode node)
        {
            var sequenceNode = (SequenceNode)node.MidNode;
            var array = Build(scope, (ExpressionNode)sequenceNode.Children[2]);

            return AstNode.ArrayLength(array);
        }

        private static AstNode ParseReal(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (double.TryParse(valueNode.Value, out double value))
                return AstNode.Constant(value);

            throw new ArgumentException(nameof(node));
        }

        private static AstNode ParseIntegral(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (int.TryParse(valueNode.Value, out int value))
                return AstNode.Constant(value);

            throw new ArgumentException(nameof(node));
        }

        private static AstNode ParseBoolean(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (bool.TryParse(valueNode.Value, out bool value))
                return AstNode.Constant(value);

            throw new ArgumentException(nameof(node));
        }

        private static AstNode ParseIdentifier(SymbolScope scope, ExpressionNode node)
        {
            var valueNode = (ValueNode)node.MidNode;
            if (scope.TryGetSymbol<AstNode>(valueNode.Value, out var symbol))
                return symbol.Value;

            throw new ArgumentException($"Missing {nameof(AstNode)} symbol, {valueNode.Value}");
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

                if (TypeHelper.IsReal(left.Type) && TypeHelper.IsIntegral(right.Type))
                    right = AstNode.Convert(right, left.Type);
                else if (TypeHelper.IsIntegral(left.Type) && TypeHelper.IsReal(right.Type))
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

            if (TypeHelper.IsIntegral(mid.Type) && TypeHelper.IsReal(right.Type))
                mid = AstNode.Convert(mid, right.Type);
            else if (TypeHelper.IsReal(mid.Type) && TypeHelper.IsIntegral(right.Type))
                right = AstNode.Convert(right, mid.Type);

            return AstNode.Conditional(left, mid, right);
        }
    }
}
