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

            var methodInfo = Compile("AssemblyName", "ModuleName", "TypeName", "MethodName", typeof(T), new ParameterNode[] { });

            return (Func<T>)methodInfo.CreateDelegate(typeof(Func<T>));
        }

        public static Func<TContext, TResult> CompileFunction<TContext, TResult>(ExpressionNode node)
        {
            var parameters = new ParameterNode[] { AstNode.Parameter(typeof(TContext)) };
            var scope = AstScope.FromMembers(parameters[0]);

            var ast = AstBuilder.Build(scope, node);
            ast = AstBuilder.ConvertIfRequired(ast, typeof(TResult));

            var methodInfo = Compile("AssemblyName", "ModuleName", "TypeName", "MethodName", typeof(TResult), parameters);

            return (Func<TContext, TResult>)methodInfo.CreateDelegate(typeof(Func<TContext, TResult>));
        }

        private static MethodInfo Compile(
            string assemblyName,
            string moduleName,
            string typeName,
            string methodName,
            Type returnType,
            ParameterNode[] parameters)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            var parameterTypes = parameters.Select(x => x.Type).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, returnType, parameterTypes);

            var ilGenerator = methodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldc_R8, 5.5);
            ilGenerator.Emit(OpCodes.Ret);

            var loadedType = typeBuilder.CreateType();
            var loadedMethodInfo = loadedType.GetMethod(methodBuilder.Name, parameterTypes);

            return loadedMethodInfo;
        }
    }
}
