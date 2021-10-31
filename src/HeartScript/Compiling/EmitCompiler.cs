using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;
using HeartScript.Parsing;

namespace HeartScript.Compiling
{
    public static class EmitCompiler
    {
        public static Func<T> CompileFunction<T>(IParseNode node)
        {
            var scope = AstScope.Empty();

            var ast = AstBuilder.Build(scope, node);
            ast = AstBuilder.ConvertIfRequired(ast, typeof(T));

            return Compile<Func<T>>(
                "AssemblyName",
                "ModuleName",
                "TypeName",
                "MethodName",
                ast,
                scope,
                typeof(T),
                new ParameterNode[0]);
        }

        public static Func<TContext, TResult> CompileFunction<TContext, TResult>(IParseNode node)
        {
            var parameters = new ParameterNode[] { AstNode.Parameter(0, typeof(TContext)) };
            var scope = AstScope.FromMembers(parameters[0]);

            var ast = AstBuilder.Build(scope, node);
            ast = AstBuilder.ConvertIfRequired(ast, typeof(TResult));

            return Compile<Func<TContext, TResult>>(
                "AssemblyName",
                "ModuleName",
                "TypeName",
                "MethodName",
                ast,
                scope,
                typeof(TResult),
                parameters);
        }

        private static T Compile<T>(
            string assemblyName,
            string moduleName,
            string typeName,
            string methodName,
            AstNode ast,
            AstScope scope,
            Type returnType,
            ParameterNode[] parameters)
            where T : Delegate
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            var parameterTypes = parameters.Select(x => x.Type).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, returnType, parameterTypes);

            var ilGenerator = methodBuilder.GetILGenerator();
            Emit(ilGenerator, scope, ast);
            ilGenerator.Emit(OpCodes.Ret);

            var loadedType = typeBuilder.CreateType();
            var loadedMethodInfo = loadedType.GetMethod(methodBuilder.Name, parameterTypes);

            return (T)loadedMethodInfo.CreateDelegate(typeof(T));
        }

        private static void Emit(ILGenerator ilGenerator, AstScope scope, AstNode node)
        {
            scope.AssertAllowed(node.Type);
            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(ilGenerator, scope, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(ilGenerator, scope, binaryNode); break;
                case UnaryNode unaryNode: EmitUnary(ilGenerator, scope, unaryNode); break;
                case ConditionalNode conditionalNode: EmitConditional(ilGenerator, scope, conditionalNode); break;
                case CallNode callNode: EmitCall(ilGenerator, scope, callNode); break;
                case ParameterNode parameterNode: EmitParameter(ilGenerator, scope, parameterNode); break;
                case MemberNode memberNode: EmitMemberAccess(ilGenerator, scope, memberNode); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitConstant(ILGenerator ilGenerator, AstScope scope, ConstantNode node)
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

        private static void EmitParameter(ILGenerator ilGenerator, AstScope scope, ParameterNode node)
        {
            ilGenerator.Emit(OpCodes.Ldarg, node.ParameterIndex);
        }
    }
}
