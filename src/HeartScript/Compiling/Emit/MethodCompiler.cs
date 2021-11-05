using System;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public static class MethodCompiler
    {
        public static void EmitMethod(TypeBuilder typeBuilder, AstScope scope, MethodInfoNode node)
        {
            var methodBuilder = typeBuilder.DefineMethod(node.Name, MethodAttributes.Public | MethodAttributes.Static, node.ReturnType, node.ParameterTypes);
            var ilGenerator = methodBuilder.GetILGenerator();

            Emit(ilGenerator, scope, node.Body);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void Emit(ILGenerator ilGenerator, AstScope scope, AstNode node)
        {
            scope.AssertAllowed(node.Type);
            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(ilGenerator, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(ilGenerator, scope, binaryNode); break;
                case UnaryNode unaryNode: EmitUnary(ilGenerator, scope, unaryNode); break;
                case ConditionalNode conditionalNode: EmitConditional(ilGenerator, scope, conditionalNode); break;
                case CallNode callNode: EmitCall(ilGenerator, scope, callNode); break;
                case ParameterNode parameterNode: EmitParameter(ilGenerator, parameterNode); break;
                case MemberNode memberNode: EmitMemberAccess(ilGenerator, scope, memberNode); break;
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

        private static void EmitBinary(ILGenerator ilGenerator, AstScope scope, BinaryNode node)
        {
            Emit(ilGenerator, scope, node.Left);
            Emit(ilGenerator, scope, node.Right);

            switch (node.NodeType)
            {
                case AstType.Multiply: ilGenerator.Emit(OpCodes.Mul); break;
                case AstType.Divide: ilGenerator.Emit(OpCodes.Div); break;
                case AstType.Add: ilGenerator.Emit(OpCodes.Add); break;
                case AstType.Subtract: ilGenerator.Emit(OpCodes.Sub); break;
                case AstType.LessThanOrEqual:
                    {
                        ilGenerator.Emit(OpCodes.Cgt);
                        ilGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ilGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.GreaterThanOrEqual:
                    {
                        ilGenerator.Emit(OpCodes.Clt);
                        ilGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ilGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.LessThan: ilGenerator.Emit(OpCodes.Clt); break;
                case AstType.GreaterThan: ilGenerator.Emit(OpCodes.Cgt); break;
                case AstType.Equal: ilGenerator.Emit(OpCodes.Ceq); break;
                case AstType.NotEqual:
                    {
                        ilGenerator.Emit(OpCodes.Ceq);
                        ilGenerator.Emit(OpCodes.Ldc_I4, 0);
                        ilGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.And: ilGenerator.Emit(OpCodes.And); break;
                case AstType.ExclusiveOr: ilGenerator.Emit(OpCodes.Xor); break;
                case AstType.Or: ilGenerator.Emit(OpCodes.Or); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitUnary(ILGenerator ilGenerator, AstScope scope, UnaryNode node)
        {
            Emit(ilGenerator, scope, node.Operand);

            switch (node.NodeType)
            {
                case AstType.Convert:
                    {
                        if (node.Operand.Type != typeof(int) || node.Type != typeof(double))
                            throw new NotImplementedException();

                        ilGenerator.Emit(OpCodes.Conv_R8);
                    }
                    break;
                case AstType.UnaryPlus: break;
                case AstType.Negate: ilGenerator.Emit(OpCodes.Neg); break;
                case AstType.Not: ilGenerator.Emit(OpCodes.Not); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitConditional(ILGenerator ilGenerator, AstScope scope, ConditionalNode node)
        {
            var ifFalseLabel = ilGenerator.DefineLabel();
            var endLabel = ilGenerator.DefineLabel();

            Emit(ilGenerator, scope, node.Test);
            ilGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

            //true
            Emit(ilGenerator, scope, node.IfTrue);
            ilGenerator.Emit(OpCodes.Br, endLabel);

            //false
            ilGenerator.MarkLabel(ifFalseLabel);
            Emit(ilGenerator, scope, node.IfFalse);

            ilGenerator.MarkLabel(endLabel);
        }

        private static void EmitCall(ILGenerator ilGenerator, AstScope scope, CallNode node)
        {
            if (node.Instance != null)
                Emit(ilGenerator, scope, node.Instance);
            else
                scope.AssertAllowed(node.MethodInfo.ReflectedType);

            foreach (var parameter in node.Parameters)
            {
                Emit(ilGenerator, scope, parameter);
            }

            ilGenerator.EmitCall(OpCodes.Call, node.MethodInfo, null);
        }

        private static void EmitMemberAccess(ILGenerator ilGenerator, AstScope scope, MemberNode node)
        {
            if (node.Instance != null)
                Emit(ilGenerator, scope, node.Instance);
            else
                scope.AssertAllowed(node.Member.ReflectedType);

            switch (node.Member)
            {
                case FieldInfo fieldInfo: ilGenerator.Emit(OpCodes.Ldfld, fieldInfo); break;
                case PropertyInfo propertyInfo: ilGenerator.EmitCall(OpCodes.Call, propertyInfo.GetMethod, null); break;
                default: throw new NotImplementedException();
            };
        }

        private static void EmitParameter(ILGenerator ilGenerator, ParameterNode node)
        {
            ilGenerator.Emit(OpCodes.Ldarg, node.ParameterIndex);
        }
    }
}
