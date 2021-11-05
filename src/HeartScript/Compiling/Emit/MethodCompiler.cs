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

        public static void EmitMethod(System.Reflection.Emit.TypeBuilder typeBuilder, MethodInfoNode node)
        {
            var methodBuilder = typeBuilder.DefineMethod(node.Name, MethodAttributes.Public | MethodAttributes.Static, node.ReturnType, node.ParameterTypes);
            var ilGenerator = methodBuilder.GetILGenerator();

            var scope = new PathScope();
            EmitStatement(ilGenerator, scope, node.Body);

            if (!scope.Return)
                throw new Exception("Not all code paths return a value");
        }

        private static void EmitStatement(ILGenerator ilGenerator, PathScope scope, AstNode node)
        {
            switch (node)
            {
                case BlockNode blockNode: EmitBlock(ilGenerator, scope, blockNode); break; ;
                case ReturnNode returnNode: EmitReturn(ilGenerator, scope, returnNode); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitExpression(ILGenerator ilGenerator, AstNode node)
        {
            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(ilGenerator, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(ilGenerator, binaryNode); break;
                case UnaryNode unaryNode: EmitUnary(ilGenerator, unaryNode); break;
                case ConditionalNode conditionalNode: EmitConditional(ilGenerator, conditionalNode); break;
                case CallNode callNode: EmitCall(ilGenerator, callNode); break;
                case ParameterNode parameterNode: EmitParameter(ilGenerator, parameterNode); break;
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

        private static void EmitBinary(ILGenerator ilGenerator, BinaryNode node)
        {
            EmitExpression(ilGenerator, node.Left);
            EmitExpression(ilGenerator, node.Right);

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

        private static void EmitUnary(ILGenerator ilGenerator, UnaryNode node)
        {
            EmitExpression(ilGenerator, node.Operand);

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

        private static void EmitConditional(ILGenerator ilGenerator, ConditionalNode node)
        {
            var ifFalseLabel = ilGenerator.DefineLabel();
            var endLabel = ilGenerator.DefineLabel();

            EmitExpression(ilGenerator, node.Test);
            ilGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

            //true
            EmitExpression(ilGenerator, node.IfTrue);
            ilGenerator.Emit(OpCodes.Br, endLabel);

            //false
            ilGenerator.MarkLabel(ifFalseLabel);
            EmitExpression(ilGenerator, node.IfFalse);

            ilGenerator.MarkLabel(endLabel);
        }

        private static void EmitCall(ILGenerator ilGenerator, CallNode node)
        {
            if (node.Instance != null)
                EmitExpression(ilGenerator, node.Instance);

            foreach (var parameter in node.Parameters)
            {
                EmitExpression(ilGenerator, parameter);
            }

            ilGenerator.EmitCall(OpCodes.Call, node.MethodInfo, null);
        }


        private static void EmitParameter(ILGenerator ilGenerator, ParameterNode node)
        {
            ilGenerator.Emit(OpCodes.Ldarg, node.ParameterIndex);
        }

        private static void EmitMemberAccess(ILGenerator ilGenerator, MemberAccessNode node)
        {
            if (node.Instance != null)
                EmitExpression(ilGenerator, node.Instance);

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
                EmitStatement(ilGenerator, scope, statement);
            }
        }

        private static void EmitReturn(ILGenerator ilGenerator, PathScope scope, ReturnNode node)
        {
            if (node.Node != null)
                EmitExpression(ilGenerator, node.Node);

            ilGenerator.Emit(OpCodes.Ret);
            scope.Return = true;
        }
    }
}
