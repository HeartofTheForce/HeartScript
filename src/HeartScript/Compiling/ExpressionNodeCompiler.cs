using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;
using HeartScript.Expressions;

namespace HeartScript.Compiling
{
    public static class ExpressionNodeCompiler
    {
        public static Func<T> CompileFunction<T>(ExpressionNode node)
        {
            var scope = AstScope.Empty();

            var ast = AstBuilder.Build(scope, node);
            ast = AstBuilder.ConvertIfRequired(ast, typeof(T));

            var methodInfo = Compile("AssemblyName", "ModuleName", "TypeName", "MethodName", ast, typeof(T), new ParameterNode[] { });

            return (Func<T>)methodInfo.CreateDelegate(typeof(Func<T>));
        }

        public static Func<TContext, TResult> CompileFunction<TContext, TResult>(ExpressionNode node)
        {
            var parameters = new ParameterNode[] { AstNode.Parameter(typeof(TContext)) };
            var scope = AstScope.FromMembers(parameters[0]);

            var ast = AstBuilder.Build(scope, node);
            ast = AstBuilder.ConvertIfRequired(ast, typeof(TResult));

            var methodInfo = Compile("AssemblyName", "ModuleName", "TypeName", "MethodName", ast, typeof(TResult), parameters);

            return (Func<TContext, TResult>)methodInfo.CreateDelegate(typeof(Func<TContext, TResult>));
        }

        private static MethodInfo Compile(
            string assemblyName,
            string moduleName,
            string typeName,
            string methodName,
            AstNode ast,
            Type returnType,
            ParameterNode[] parameters)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            var parameterTypes = parameters.Select(x => x.Type).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, returnType, parameterTypes);

            var ilGenerator = methodBuilder.GetILGenerator();
            Emit(ilGenerator, ast);
            ilGenerator.Emit(OpCodes.Ret);

            var loadedType = typeBuilder.CreateType();
            var loadedMethodInfo = loadedType.GetMethod(methodBuilder.Name, parameterTypes);

            return loadedMethodInfo;
        }

        private static void Emit(ILGenerator ilGenerator, AstNode node)
        {
            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(ilGenerator, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(ilGenerator, binaryNode); break;
                case UnaryNode unaryNode: EmitUnary(ilGenerator, unaryNode); break;
                case ConditionalNode conditionalNode: EmitConditional(ilGenerator, conditionalNode); break;
                case CallNode callNode: EmitCall(ilGenerator, callNode); break;
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
            Emit(ilGenerator, node.Left);
            Emit(ilGenerator, node.Right);

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
            Emit(ilGenerator, node.Operand);

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

            Emit(ilGenerator, node.Test);
            ilGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

            //true
            Emit(ilGenerator, node.IfTrue);
            ilGenerator.Emit(OpCodes.Br, endLabel);

            //false
            ilGenerator.MarkLabel(ifFalseLabel);
            Emit(ilGenerator, node.IfFalse);

            ilGenerator.MarkLabel(endLabel);
        }

        private static void EmitCall(ILGenerator ilGenerator, CallNode node)
        {
            if (node.Instance != null)
                Emit(ilGenerator, node.Instance);

            foreach (var parameter in node.Parameters)
            {
                Emit(ilGenerator, parameter);
            }

            ilGenerator.EmitCall(OpCodes.Call, node.MethodInfo, null);
        }
    }
}
