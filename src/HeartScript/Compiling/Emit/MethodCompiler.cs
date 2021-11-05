using System;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public static class MethodCompiler
    {
        private class PathScope
        {
            public bool Return { get; set; }

            public void Merge(PathScope source)
            {
                if (source.Return)
                    Return = true;
            }
        }

        public static void EmitMethod(TypeBuilder typeBuilder, MethodInfoNode node)
        {
            var methodBuilder = typeBuilder.DefineMethod(node.Name, MethodAttributes.Public | MethodAttributes.Static, node.ReturnType, node.ParameterTypes);
            var ilGenerator = methodBuilder.GetILGenerator();

            var scope = new PathScope();
            Emit(ilGenerator, scope, node.Body);

            if (!scope.Return)
                throw new Exception("Not all code paths return a value");
        }

        private static void Emit(ILGenerator ilGenerator, PathScope scope, AstNode node)
        {
            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(ilGenerator, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(ilGenerator, scope, binaryNode); break;
                case UnaryNode unaryNode: EmitUnary(ilGenerator, scope, unaryNode); break;
                case ConditionalNode conditionalNode: EmitConditional(ilGenerator, scope, conditionalNode); break;
                case CallNode callNode: EmitCall(ilGenerator, scope, callNode); break;
                case ParameterNode parameterNode: EmitParameter(ilGenerator, parameterNode); break;
                case MemberAccessNode memberAccessNode: EmitMemberAccess(ilGenerator, scope, memberAccessNode); break;
                case BlockNode blockNode: EmitBlock(ilGenerator, scope, blockNode); break;
                case ReturnNode returnNode: EmitReturn(ilGenerator, scope, returnNode); break;
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

        private static void EmitBinary(ILGenerator ilGenerator, PathScope scope, BinaryNode node)
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

        private static void EmitUnary(ILGenerator ilGenerator, PathScope scope, UnaryNode node)
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

        private static void EmitConditional(ILGenerator ilGenerator, PathScope scope, ConditionalNode node)
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

        private static void EmitCall(ILGenerator ilGenerator, PathScope scope, CallNode node)
        {
            if (node.Instance != null)
                Emit(ilGenerator, scope, node.Instance);

            foreach (var parameter in node.Parameters)
            {
                Emit(ilGenerator, scope, parameter);
            }

            ilGenerator.EmitCall(OpCodes.Call, node.MethodInfo, null);
        }


        private static void EmitParameter(ILGenerator ilGenerator, ParameterNode node)
        {
            ilGenerator.Emit(OpCodes.Ldarg, node.ParameterIndex);
        }

        private static void EmitMemberAccess(ILGenerator ilGenerator, PathScope scope, MemberAccessNode node)
        {
            if (node.Instance != null)
                Emit(ilGenerator, scope, node.Instance);

            switch (node.Member)
            {
                case FieldInfo fieldInfo: ilGenerator.Emit(OpCodes.Ldfld, fieldInfo); break;
                case PropertyInfo propertyInfo: ilGenerator.EmitCall(OpCodes.Call, propertyInfo.GetMethod, null); break;
                default: throw new NotImplementedException();
            };
        }

        private static void EmitBlock(ILGenerator ilGenerator, PathScope scope, BlockNode node)
        {
            foreach (var statement in node.Nodes)
            {
                Emit(ilGenerator, scope, statement);
            }
        }

        private static void EmitReturn(ILGenerator ilGenerator, PathScope scope, ReturnNode node)
        {
            if (node.Node != null)
                Emit(ilGenerator, scope, node.Node);

            ilGenerator.Emit(OpCodes.Ret);
            scope.Return = true;
        }
    }
}
