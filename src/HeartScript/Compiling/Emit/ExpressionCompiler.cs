using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public static class ExpressionCompiler
    {
        private static readonly HashSet<AstType> s_validStatmentExpressions = new HashSet<AstType>()
        {
            AstType.Assign,
            AstType.PostIncrement,
            AstType.PostDecrement,
            AstType.Call,
        };

        public static void EmitExpression(MethodBodyContext ctx, AstNode node, bool isStatement)
        {
            if (isStatement && !s_validStatmentExpressions.Contains(node.NodeType))
                throw new ArgumentException("Invalid statement expression");

            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(ctx, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(ctx, binaryNode, isStatement); break;
                case UnaryNode unaryNode: EmitUnary(ctx, unaryNode, isStatement); break;
                case ConditionalNode conditionalNode: EmitConditional(ctx, conditionalNode); break;
                case CallNode callNode: EmitCall(ctx, callNode); break;
                case ParameterNode parameterNode: EmitParameter(ctx, parameterNode); break;
                case VariableNode variableNode: EmitVariable(ctx, variableNode); break;
                case MemberAccessNode memberAccessNode: EmitMemberAccess(ctx, memberAccessNode); break;
                case ArrayConstructorNode arrayConstructorNode: EmitArrayConstructor(ctx, arrayConstructorNode); break;
                case ArrayIndexNode arrayIndexNode: EmitArrayRead(ctx, arrayIndexNode); break;
                case ArrayLengthNode arrayLengthNode: EmitArrayLength(ctx, arrayLengthNode); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitConstant(MethodBodyContext ctx, ConstantNode node)
        {
            if (node.Value == null)
                throw new ArgumentException(nameof(node.Value));

            if (node.Type == typeof(int))
            {
                ctx.ILGenerator.Emit(OpCodes.Ldc_I4, (int)node.Value);
                return;
            }

            if (node.Type == typeof(double))
            {
                ctx.ILGenerator.Emit(OpCodes.Ldc_R8, (double)node.Value);
                return;
            }

            if (node.Type == typeof(bool))
            {
                ctx.ILGenerator.Emit(OpCodes.Ldc_I4, (bool)node.Value ? 1 : 0);
                return;
            }

            throw new NotImplementedException();
        }

        private static void EmitBinary(MethodBodyContext ctx, BinaryNode node, bool isStatement)
        {
            switch (node.NodeType)
            {
                case AstType.Multiply:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Mul);
                    }
                    break;
                case AstType.Divide:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Div);
                    }
                    break;
                case AstType.Add:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Add);
                    }
                    break;
                case AstType.Subtract:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Sub);
                    }
                    break;
                case AstType.LessThanOrEqual:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Cgt);
                        ctx.ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ctx.ILGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.GreaterThanOrEqual:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Clt);
                        ctx.ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ctx.ILGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.LessThan:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Clt);
                    }
                    break;
                case AstType.GreaterThan:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Cgt);
                    }
                    break;
                case AstType.Equal:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.NotEqual:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Ceq);
                        ctx.ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ctx.ILGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.And:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.And);
                    }
                    break;
                case AstType.ExclusiveOr:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Xor);
                    }
                    break;
                case AstType.Or:
                    {
                        EmitExpression(ctx, node.Left, false);
                        EmitExpression(ctx, node.Right, false);
                        ctx.ILGenerator.Emit(OpCodes.Or);
                    }
                    break;
                case AstType.Assign:
                    {
                        EmitSet(ctx, node.Left, node.Right, false, !isStatement, "The left-hand side of an assignment must be a variable or parameter");
                    }
                    break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitUnary(MethodBodyContext ctx, UnaryNode node, bool isStatement)
        {
            switch (node.NodeType)
            {
                case AstType.Convert:
                    {
                        EmitExpression(ctx, node.Operand, false);
                        var opCode = TypeHelper.ConvertOpCode(node.Operand.Type, node.Type);
                        ctx.ILGenerator.Emit(opCode);
                    }
                    break;
                case AstType.UnaryPlus:
                    {
                        EmitExpression(ctx, node.Operand, false);
                    }
                    break;
                case AstType.Negate:
                    {
                        EmitExpression(ctx, node.Operand, false);
                        ctx.ILGenerator.Emit(OpCodes.Neg);
                    }
                    break;
                case AstType.Not:
                    {
                        EmitExpression(ctx, node.Operand, false);
                        ctx.ILGenerator.Emit(OpCodes.Not);
                    }
                    break;
                case AstType.PostIncrement:
                    {
                        PostCrement(ctx, node, isStatement, AstNode.Add);
                    }
                    break;
                case AstType.PostDecrement:
                    {
                        PostCrement(ctx, node, isStatement, AstNode.Subtract);
                    }
                    break;
                default: throw new NotImplementedException();
            }
        }

        private static void PostCrement(MethodBodyContext ctx, UnaryNode node, bool isStatement, Func<AstNode, AstNode, AstNode> nodeBuilder)
        {
            AstNode constantNode;
            if (node.Operand.Type == typeof(int))
                constantNode = AstNode.Constant(1);
            else if (node.Operand.Type == typeof(double))
                constantNode = AstNode.Constant(1.0);
            else
                throw new ArgumentException(nameof(node.Operand.Type));

            var valueNode = nodeBuilder(node.Operand, constantNode);
            EmitSet(ctx, node.Operand, valueNode, !isStatement, false, "The operand of an increment or decrement operator must be a variable or parameter");
        }

        private static void EmitSet(MethodBodyContext ctx, AstNode targetNode, AstNode valueNode, bool pushTarget, bool pushValue, string invalidTypeMessage)
        {
            switch (targetNode)
            {
                case ParameterNode parameterNode:
                    {
                        if(pushTarget)
                            EmitParameter(ctx, parameterNode);

                        EmitExpression(ctx, valueNode, false);

                        if (pushValue)
                            ctx.ILGenerator.Emit(OpCodes.Dup);

                        ctx.ILGenerator.Emit(OpCodes.Starg, parameterNode.Index);
                    }
                    break;
                case VariableNode variableNode:
                    {
                        if(pushTarget)
                            EmitVariable(ctx, variableNode);

                        EmitExpression(ctx, valueNode, false);

                        if (pushValue)
                            ctx.ILGenerator.Emit(OpCodes.Dup);

                        ctx.ILGenerator.Emit(OpCodes.Stloc, variableNode.Index);
                    }
                    break;
                case ArrayIndexNode arrayIndexNode:
                    {
                        if(pushTarget)
                            EmitArrayRead(ctx, arrayIndexNode);

                        EmitExpression(ctx, arrayIndexNode.Array, false);
                        EmitExpression(ctx, arrayIndexNode.Index, false);
                        EmitExpression(ctx, valueNode, false);

                        LocalBuilder? localBuilder = null;
                        if (pushValue)
                        {
                            localBuilder = ctx.GetSharedLocal(valueNode.Type);
                            ctx.ILGenerator.Emit(OpCodes.Dup);
                            ctx.ILGenerator.Emit(OpCodes.Stloc, localBuilder);
                        }

                        ctx.ILGenerator.Emit(OpCodes.Stelem, targetNode.Type);

                        if (localBuilder != null)
                            ctx.ILGenerator.Emit(OpCodes.Ldloc, localBuilder);
                    }
                    break;
                default: throw new ArgumentException(invalidTypeMessage);
            }
        }

        private static void EmitConditional(MethodBodyContext ctx, ConditionalNode node)
        {
            var ifFalseLabel = ctx.ILGenerator.DefineLabel();
            var endLabel = ctx.ILGenerator.DefineLabel();

            EmitExpression(ctx, node.Condition, false);
            ctx.ILGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

            //true
            EmitExpression(ctx, node.IfTrue, false);
            ctx.ILGenerator.Emit(OpCodes.Br, endLabel);

            //false
            ctx.ILGenerator.MarkLabel(ifFalseLabel);
            EmitExpression(ctx, node.IfFalse, false);

            ctx.ILGenerator.MarkLabel(endLabel);
        }

        private static void EmitCall(MethodBodyContext ctx, CallNode node)
        {
            if (node.Instance != null)
                EmitExpression(ctx, node.Instance, false);

            foreach (var parameter in node.Parameters)
            {
                EmitExpression(ctx, parameter, false);
            }

            ctx.ILGenerator.EmitCall(OpCodes.Call, node.MethodInfo, null);
        }


        private static void EmitParameter(MethodBodyContext ctx, ParameterNode node)
        {
            ctx.ILGenerator.Emit(OpCodes.Ldarg, node.Index);
        }

        private static void EmitVariable(MethodBodyContext ctx, VariableNode node)
        {
            ctx.ILGenerator.Emit(OpCodes.Ldloc, node.Index);
        }

        private static void EmitMemberAccess(MethodBodyContext ctx, MemberAccessNode node)
        {
            if (node.Instance != null)
                EmitExpression(ctx, node.Instance, false);

            switch (node.Member)
            {
                case FieldInfo fieldInfo: ctx.ILGenerator.Emit(OpCodes.Ldfld, fieldInfo); break;
                case PropertyInfo propertyInfo: ctx.ILGenerator.EmitCall(OpCodes.Call, propertyInfo.GetMethod, null); break;
                default: throw new NotImplementedException();
            };
        }

        private static void EmitArrayConstructor(MethodBodyContext ctx, ArrayConstructorNode node)
        {
            EmitExpression(ctx, node.Length, false);
            ctx.ILGenerator.Emit(OpCodes.Newarr, node.Type);
        }

        private static void EmitArrayRead(MethodBodyContext ctx, ArrayIndexNode node)
        {
            EmitExpression(ctx, node.Array, false);
            EmitExpression(ctx, node.Index, false);
            ctx.ILGenerator.Emit(OpCodes.Ldelem, node.Type);
        }

        private static void EmitArrayLength(MethodBodyContext ctx, ArrayLengthNode node)
        {
            EmitExpression(ctx, node.Array, false);
            ctx.ILGenerator.Emit(OpCodes.Ldlen);
        }
    }
}
