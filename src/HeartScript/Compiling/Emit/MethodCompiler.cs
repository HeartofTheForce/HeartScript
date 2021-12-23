using System;
using System.Reflection;
using System.Reflection.Emit;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public static class MethodCompiler
    {
        private class BasicBlock
        {
            public bool Return { get; set; }
        }

        public static void EmitMethod(System.Reflection.Emit.TypeBuilder typeBuilder, MethodInfoNode node)
        {
            var methodBuilder = typeBuilder.DefineMethod(node.Name, MethodAttributes.Public | MethodAttributes.Static, node.ReturnType, node.ParameterTypes);
            var ilGenerator = methodBuilder.GetILGenerator();

            foreach (var variableNode in node.Variables)
            {
                ilGenerator.DeclareLocal(variableNode.Type);
            }

            var basicBlock = new BasicBlock();
            EmitStatement(ilGenerator, basicBlock, node.Body);

            if (!basicBlock.Return)
                throw new Exception("Not all code paths return a value");
        }

        private static void EmitStatement(ILGenerator ilGenerator, BasicBlock basicBlock, AstNode node)
        {
            switch (node)
            {
                case ReturnNode returnNode: EmitReturn(ilGenerator, basicBlock, returnNode); break;
                case BlockNode blockNode: EmitBlock(ilGenerator, basicBlock, blockNode); break;
                default: ExpressionCompiler.EmitExpression(ilGenerator, node); break;
            }
        }

        private static void EmitReturn(ILGenerator ilGenerator, BasicBlock basicBlock, ReturnNode node)
        {
            if (node.Node != null)
                ExpressionCompiler.EmitExpression(ilGenerator, node.Node);

            ilGenerator.Emit(OpCodes.Ret);
            basicBlock.Return = true;
        }

        private static void EmitBlock(ILGenerator ilGenerator, BasicBlock basicBlock, BlockNode node)
        {
            foreach (var statement in node.Statements)
            {
                EmitStatement(ilGenerator, basicBlock, statement);
            }
        }
    }
}
