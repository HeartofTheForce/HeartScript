using System;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;
using Heart.Parsing;

namespace HeartScript.Compiling.Emit
{
    public static class EmitCompiler
    {
        public static T CompileFunction<T>(IParseNode node)
            where T : Delegate
        {
            var methodInfo = CompileFunction(node);
            return (T)methodInfo.CreateDelegate(typeof(T));
        }

        public static MethodInfo CompileFunction(IParseNode node)
        {
            var scope = new SymbolScope();
            var ast = AstBuilder.Build(scope, node);

            return Compile(
                "AssemblyName",
                "ModuleName",
                "TypeName",
                "main",
                ast);
        }

        private static MethodInfo Compile(
            string assemblyName,
            string moduleName,
            string typeName,
            string methodName,
            AstNode ast)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            Emit(typeBuilder, ast);

            var loadedType = typeBuilder.CreateType();
            var loadedMethodInfo = loadedType.GetMethod(methodName);

            return loadedMethodInfo;
        }

        private static void Emit(System.Reflection.Emit.TypeBuilder typeBuilder, AstNode node)
        {
            switch (node)
            {
                case MethodInfoNode methodInfoNode: MethodCompiler.EmitMethod(typeBuilder, methodInfoNode); break;
                default: EmitWrap(typeBuilder, node); break;
            }
        }

        private static void EmitWrap(System.Reflection.Emit.TypeBuilder typeBuilder, AstNode node)
        {
            var blockNode = AstNode.Block(new AstNode[] { AstNode.Return(node) }, node.Type);
            var methodNode = new MethodInfoNode("main", new Type[] { }, blockNode);
            Emit(typeBuilder, methodNode);
        }
    }
}
