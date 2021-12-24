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
        };

        public static void EmitExpression(ILGenerator ilGenerator, AstNode node, bool isStatement)
        {
            if (isStatement && !s_validStatmentExpressions.Contains(node.NodeType))
                throw new ArgumentException("Invalid statement expression");

            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(ilGenerator, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(ilGenerator, binaryNode, isStatement); break;
                case UnaryNode unaryNode: EmitUnary(ilGenerator, unaryNode, isStatement); break;
                case ConditionalNode conditionalNode: EmitConditional(ilGenerator, conditionalNode); break;
                case CallNode callNode: EmitCall(ilGenerator, callNode); break;
                case ParameterNode parameterNode: EmitParameter(ilGenerator, parameterNode); break;
                case VariableNode variableNode: EmitVariable(ilGenerator, variableNode); break;
                case MemberAccessNode memberAccessNode: EmitMemberAccess(ilGenerator, memberAccessNode); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitConstant(ILGenerator ilGenerator, ConstantNode node)
        {
            if (node.Value == null)
                throw new ArgumentException(nameof(node.Value));

            if (node.Type == typeof(int))
            {
                ilGenerator.Emit(OpCodes.Ldc_I4, (int)node.Value);
                return;
            }

            if (node.Type == typeof(double))
            {
                ilGenerator.Emit(OpCodes.Ldc_R8, (double)node.Value);
                return;
            }

            if (node.Type == typeof(bool))
            {
                ilGenerator.Emit(OpCodes.Ldc_I4, (bool)node.Value ? 1 : 0);
                return;
            }

            throw new NotImplementedException();
        }

        private static void EmitBinary(ILGenerator ilGenerator, BinaryNode node, bool isStatement)
        {
            switch (node.NodeType)
            {
                case AstType.Multiply:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Mul);
                    }
                    break;
                case AstType.Divide:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Div);
                    }
                    break;
                case AstType.Add:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Add);
                    }
                    break;
                case AstType.Subtract:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Sub);
                    }
                    break;
                case AstType.LessThanOrEqual:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Cgt);
                        ilGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ilGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.GreaterThanOrEqual:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Clt);
                        ilGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ilGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.LessThan:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Clt);
                    }
                    break;
                case AstType.GreaterThan:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Cgt);
                    }
                    break;
                case AstType.Equal:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.NotEqual:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Ceq);
                        ilGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ilGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.And:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.And);
                    }
                    break;
                case AstType.ExclusiveOr:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Xor);
                    }
                    break;
                case AstType.Or:
                    {
                        EmitExpression(ilGenerator, node.Left, false);
                        EmitExpression(ilGenerator, node.Right, false);
                        ilGenerator.Emit(OpCodes.Or);
                    }
                    break;
                case AstType.Assign:
                    {
                        EmitExpression(ilGenerator, node.Right, false);
                        if (!isStatement)
                            ilGenerator.Emit(OpCodes.Dup);

                        EmitSet(ilGenerator, node.Left, "The left-hand side of an assignment must be a variable or parameter");
                    }
                    break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitUnary(ILGenerator ilGenerator, UnaryNode node, bool isStatement)
        {
            switch (node.NodeType)
            {
                case AstType.Convert:
                    {
                        if (node.Operand.Type != typeof(int) || node.Type != typeof(double))
                            throw new ArgumentException($"Cannot convert, {node.Operand.Type} to {node.Type}");

                        EmitExpression(ilGenerator, node.Operand, false);
                        ilGenerator.Emit(OpCodes.Conv_R8);
                    }
                    break;
                case AstType.UnaryPlus:
                    {
                        EmitExpression(ilGenerator, node.Operand, false);
                    }
                    break;
                case AstType.Negate:
                    {
                        EmitExpression(ilGenerator, node.Operand, false);
                        ilGenerator.Emit(OpCodes.Neg);
                    }
                    break;
                case AstType.Not:
                    {
                        EmitExpression(ilGenerator, node.Operand, false);
                        ilGenerator.Emit(OpCodes.Not);
                    }
                    break;
                case AstType.PostIncrement:
                    {
                        EmitExpression(ilGenerator, node.Operand, false);
                        if (!isStatement)
                            ilGenerator.Emit(OpCodes.Dup);

                        if (node.Operand.Type == typeof(int))
                            ilGenerator.Emit(OpCodes.Ldc_I4, 1);
                        else if (node.Operand.Type == typeof(double))
                            ilGenerator.Emit(OpCodes.Ldc_R8, 1.0);
                        else
                            throw new ArgumentException(nameof(node.Operand.Type));

                        ilGenerator.Emit(OpCodes.Add);
                        EmitSet(ilGenerator, node.Operand, "The operand of an increment or decrement operator must be a variable or parameter");
                    }
                    break;
                case AstType.PostDecrement:
                    {
                        EmitExpression(ilGenerator, node.Operand, false);
                        if (!isStatement)
                            ilGenerator.Emit(OpCodes.Dup);

                        if (node.Operand.Type == typeof(int))
                            ilGenerator.Emit(OpCodes.Ldc_I4, 1);
                        else if (node.Operand.Type == typeof(double))
                            ilGenerator.Emit(OpCodes.Ldc_R8, 1.0);
                        else
                            throw new ArgumentException(nameof(node.Operand.Type));

                        ilGenerator.Emit(OpCodes.Sub);
                        EmitSet(ilGenerator, node.Operand, "The operand of an increment or decrement operator must be a variable or parameter");
                    }
                    break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitSet(ILGenerator ilGenerator, AstNode node, string invalidTypeMessage)
        {
            switch (node)
            {
                case ParameterNode parameterNode:
                    ilGenerator.Emit(OpCodes.Starg, parameterNode.Index); break;
                case VariableNode variableNode:
                    ilGenerator.Emit(OpCodes.Stloc, variableNode.Index); break;
                default: throw new ArgumentException(invalidTypeMessage);
            }
        }

        private static void EmitConditional(ILGenerator ilGenerator, ConditionalNode node)
        {
            var ifFalseLabel = ilGenerator.DefineLabel();
            var endLabel = ilGenerator.DefineLabel();

            EmitExpression(ilGenerator, node.Test, false);
            ilGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

            //true
            EmitExpression(ilGenerator, node.IfTrue, false);
            ilGenerator.Emit(OpCodes.Br, endLabel);

            //false
            ilGenerator.MarkLabel(ifFalseLabel);
            EmitExpression(ilGenerator, node.IfFalse, false);

            ilGenerator.MarkLabel(endLabel);
        }

        private static void EmitCall(ILGenerator ilGenerator, CallNode node)
        {
            if (node.Instance != null)
                EmitExpression(ilGenerator, node.Instance, false);

            foreach (var parameter in node.Parameters)
            {
                EmitExpression(ilGenerator, parameter, false);
            }

            ilGenerator.EmitCall(OpCodes.Call, node.MethodInfo, null);
        }


        private static void EmitParameter(ILGenerator ilGenerator, ParameterNode node)
        {
            ilGenerator.Emit(OpCodes.Ldarg, node.Index);
        }

        private static void EmitVariable(ILGenerator ilGenerator, VariableNode node)
        {
            ilGenerator.Emit(OpCodes.Ldloc, node.Index);
        }

        private static void EmitMemberAccess(ILGenerator ilGenerator, MemberAccessNode node)
        {
            if (node.Instance != null)
                EmitExpression(ilGenerator, node.Instance, false);

            switch (node.Member)
            {
                case FieldInfo fieldInfo: ilGenerator.Emit(OpCodes.Ldfld, fieldInfo); break;
                case PropertyInfo propertyInfo: ilGenerator.EmitCall(OpCodes.Call, propertyInfo.GetMethod, null); break;
                default: throw new NotImplementedException();
            };
        }
    }
}
