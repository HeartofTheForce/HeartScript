using System;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast;
using HeartScript.Ast.Nodes;
using Heart.Parsing;
using Heart.Parsing.Patterns;

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
            scope.DeclareSymbol("Math", new Symbol<Type>(true, typeof(Math)));

            AstNode ast;
            if (node is LabelNode labelNode)
                ast = Ast.TypeBuilder.Build(scope, labelNode);
            else
                throw new NotImplementedException();

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
                default: throw new NotImplementedException();
            }
        }
    }
}
