using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;
using HeartScript.Expressions;

namespace HeartScript.Compiling
{
    public class EmitScope
    {
        public ILGenerator ILGenerator { get; }

        private readonly HashSet<Type> _typeWhitelist;
        private readonly Dictionary<ParameterNode, int> _parameterMap;

        public EmitScope(ILGenerator iLGenerator, ParameterNode[] parameters)
        {
            ILGenerator = iLGenerator;

            _typeWhitelist = new HashSet<Type>()
            {
                typeof(int),
                typeof(double),
                typeof(bool),
            };

            _parameterMap = new Dictionary<ParameterNode, int>();
            for (int i = 0; i < parameters.Length; i++)
            {
                _parameterMap[parameters[i]] = i;
                _typeWhitelist.Add(parameters[i].Type);
            }
        }

        public int GetParameterIndex(ParameterNode node)
        {
            return _parameterMap[node];
        }

        public bool IsAllowed(Type type)
        {
            return _typeWhitelist.Contains(type);
        }
    }

    public static class ExpressionNodeCompiler
    {
        public static Func<T> CompileFunction<T>(ExpressionNode node)
        {
            var scope = AstScope.Empty();

            var ast = AstBuilder.Build(scope, node);
            ast = AstBuilder.ConvertIfRequired(ast, typeof(T));

            return Compile<Func<T>>("AssemblyName", "ModuleName", "TypeName", "MethodName", ast, typeof(T), new ParameterNode[0]);
        }

        public static Func<TContext, TResult> CompileFunction<TContext, TResult>(ExpressionNode node)
        {
            var parameters = new ParameterNode[] { AstNode.Parameter(typeof(TContext)) };
            var scope = AstScope.FromMembers(parameters[0]);

            var ast = AstBuilder.Build(scope, node);
            ast = AstBuilder.ConvertIfRequired(ast, typeof(TResult));

            return Compile<Func<TContext, TResult>>("AssemblyName", "ModuleName", "TypeName", "MethodName", ast, typeof(TResult), parameters);
        }

        private static T Compile<T>(
            string assemblyName,
            string moduleName,
            string typeName,
            string methodName,
            AstNode ast,
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
            var emitScope = new EmitScope(ilGenerator, parameters);
            Emit(emitScope, ast);
            ilGenerator.Emit(OpCodes.Ret);

            var loadedType = typeBuilder.CreateType();
            var loadedMethodInfo = loadedType.GetMethod(methodBuilder.Name, parameterTypes);

            return (T)loadedMethodInfo.CreateDelegate(typeof(T));
        }

        private static void Emit(EmitScope scope, AstNode node)
        {
            if (!scope.IsAllowed(node.Type))
                throw new Exception($"{node.Type} is not allowed");

            switch (node)
            {
                case ConstantNode constantNode: EmitConstant(scope, constantNode); break;
                case BinaryNode binaryNode: EmitBinary(scope, binaryNode); break;
                case UnaryNode unaryNode: EmitUnary(scope, unaryNode); break;
                case ConditionalNode conditionalNode: EmitConditional(scope, conditionalNode); break;
                case CallNode callNode: EmitCall(scope, callNode); break;
                case ParameterNode parameterNode: EmitParameter(scope, parameterNode); break;
                case MemberNode memberNode: EmitMemberAccess(scope, memberNode); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitConstant(EmitScope scope, ConstantNode node)
        {
            if (node.Value == null)
                throw new ArgumentException(nameof(node.Value));

            if (node.Type == typeof(int))
            {
                scope.ILGenerator.Emit(OpCodes.Ldc_I4, (int)node.Value);
                return;
            }

            if (node.Type == typeof(double))
            {
                scope.ILGenerator.Emit(OpCodes.Ldc_R8, (double)node.Value);
                return;
            }

            if (node.Type == typeof(bool))
            {
                scope.ILGenerator.Emit(OpCodes.Ldc_I4, (bool)node.Value ? 1 : 0);
                return;
            }

            throw new NotImplementedException();
        }

        private static void EmitBinary(EmitScope scope, BinaryNode node)
        {
            Emit(scope, node.Left);
            Emit(scope, node.Right);

            switch (node.NodeType)
            {
                case AstType.Multiply: scope.ILGenerator.Emit(OpCodes.Mul); break;
                case AstType.Divide: scope.ILGenerator.Emit(OpCodes.Div); break;
                case AstType.Add: scope.ILGenerator.Emit(OpCodes.Add); break;
                case AstType.Subtract: scope.ILGenerator.Emit(OpCodes.Sub); break;
                case AstType.LessThanOrEqual:
                    {
                        scope.ILGenerator.Emit(OpCodes.Cgt);
                        scope.ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                        scope.ILGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.GreaterThanOrEqual:
                    {
                        scope.ILGenerator.Emit(OpCodes.Clt);
                        scope.ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                        scope.ILGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.LessThan: scope.ILGenerator.Emit(OpCodes.Clt); break;
                case AstType.GreaterThan: scope.ILGenerator.Emit(OpCodes.Cgt); break;
                case AstType.Equal: scope.ILGenerator.Emit(OpCodes.Ceq); break;
                case AstType.NotEqual:
                    {
                        scope.ILGenerator.Emit(OpCodes.Ceq);
                        scope.ILGenerator.Emit(OpCodes.Ldc_I4, 0);
                        scope.ILGenerator.Emit(OpCodes.Ceq);
                    }
                    break;
                case AstType.And: scope.ILGenerator.Emit(OpCodes.And); break;
                case AstType.ExclusiveOr: scope.ILGenerator.Emit(OpCodes.Xor); break;
                case AstType.Or: scope.ILGenerator.Emit(OpCodes.Or); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitUnary(EmitScope scope, UnaryNode node)
        {
            Emit(scope, node.Operand);

            switch (node.NodeType)
            {
                case AstType.Convert:
                    {
                        if (node.Operand.Type != typeof(int) || node.Type != typeof(double))
                            throw new NotImplementedException();

                        scope.ILGenerator.Emit(OpCodes.Conv_R8);
                    }
                    break;
                case AstType.UnaryPlus: break;
                case AstType.Negate: scope.ILGenerator.Emit(OpCodes.Neg); break;
                case AstType.Not: scope.ILGenerator.Emit(OpCodes.Not); break;
                default: throw new NotImplementedException();
            }
        }

        private static void EmitConditional(EmitScope scope, ConditionalNode node)
        {
            var ifFalseLabel = scope.ILGenerator.DefineLabel();
            var endLabel = scope.ILGenerator.DefineLabel();

            Emit(scope, node.Test);
            scope.ILGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

            //true
            Emit(scope, node.IfTrue);
            scope.ILGenerator.Emit(OpCodes.Br, endLabel);

            //false
            scope.ILGenerator.MarkLabel(ifFalseLabel);
            Emit(scope, node.IfFalse);

            scope.ILGenerator.MarkLabel(endLabel);
        }

        private static void EmitCall(EmitScope scope, CallNode node)
        {
            if (node.Instance != null)
                Emit(scope, node.Instance);

            foreach (var parameter in node.Parameters)
            {
                Emit(scope, parameter);
            }

            scope.ILGenerator.EmitCall(OpCodes.Call, node.MethodInfo, null);
        }

        private static void EmitMemberAccess(EmitScope scope, MemberNode node)
        {
            if (node.Instance != null)
                Emit(scope, node.Instance);

            switch (node.Member)
            {
                case FieldInfo fieldInfo: scope.ILGenerator.Emit(OpCodes.Ldfld, fieldInfo); break;
                case PropertyInfo propertyInfo: scope.ILGenerator.EmitCall(OpCodes.Call, propertyInfo.GetMethod, null); break;
                default: throw new NotImplementedException();
            };
        }

        private static void EmitParameter(EmitScope scope, ParameterNode node)
        {
            int parameterIndex = scope.GetParameterIndex(node);
            scope.ILGenerator.Emit(OpCodes.Ldarg, parameterIndex);
        }
    }
}
