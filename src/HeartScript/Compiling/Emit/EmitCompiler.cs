using System;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;
using HeartScript.Compiling.Emit;
using HeartScript.Parsing;

namespace HeartScript.Compiling
{
    public static class EmitCompiler
    {
        public static T CompileFunction<T>(IParseNode node)
            where T : Delegate
        {
            var scope = new AstScope();
            scope.AllowType(typeof(void));
            scope.AllowType(typeof(int));
            scope.AllowType(typeof(double));
            scope.AllowType(typeof(bool));
            scope.AllowType(typeof(Math));

            var ast = AstBuilder.Build(scope, node);

            var methodInfo = Compile(
                "AssemblyName",
                "ModuleName",
                "TypeName",
                "main",
                ast,
                scope);

            return (T)methodInfo.CreateDelegate(typeof(T));
        }

        public static MethodInfo CompileFunction(IParseNode node)
        {
            var scope = new AstScope();
            scope.AllowType(typeof(void));
            scope.AllowType(typeof(int));
            scope.AllowType(typeof(double));
            scope.AllowType(typeof(bool));
            scope.AllowType(typeof(Math));

            var ast = AstBuilder.Build(scope, node);

            return Compile(
                "AssemblyName",
                "ModuleName",
                "TypeName",
                "main",
                ast,
                scope);
        }

        private static MethodInfo Compile(
            string assemblyName,
            string moduleName,
            string typeName,
            string methodName,
            AstNode ast,
            AstScope scope)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            Emit(typeBuilder, scope, ast);

            var loadedType = typeBuilder.CreateType();
            var loadedMethodInfo = loadedType.GetMethod(methodName);

            return loadedMethodInfo;
        }

        private static void Emit(TypeBuilder typeBuilder, AstScope scope, AstNode node)
        {
            scope.AssertAllowed(node.Type);
            switch (node)
            {
                case MethodNode methodNode: MethodCompiler.EmitMethod(typeBuilder, scope, methodNode); break;
                default: EmitWrap(typeBuilder, scope, node); break;
            }
        }

        private static void EmitWrap(TypeBuilder typeBuilder, AstScope scope, AstNode node)
        {
            var methodNode = new MethodNode("main", new Type[] { }, node);
            Emit(typeBuilder, scope, methodNode);
        }
    }
}
