using System;
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

        public static void EmitMethodBody(MethodBuilder methodBuilder, MethodBodyNode node)
        {
            var ilGenerator = methodBuilder.GetILGenerator();

            foreach (var variable in node.Variables)
            {
                ilGenerator.DeclareLocal(variable.Type);
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
                case LoopNode loopNode: EmitLoop(ilGenerator, basicBlock, loopNode); break;
                default: ExpressionCompiler.EmitExpression(ilGenerator, node, true); break;
            }
        }

        private static void EmitReturn(ILGenerator ilGenerator, BasicBlock basicBlock, ReturnNode node)
        {
            if (node.Node != null)
                ExpressionCompiler.EmitExpression(ilGenerator, node.Node, false);

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

        private static void EmitLoop(ILGenerator ilGenerator, BasicBlock basicBlock, LoopNode loopNode)
        {
            var loopHead = ilGenerator.DefineLabel();
            var loopCondition = ilGenerator.DefineLabel();

            if (loopNode.Initialize != null)
                EmitStatement(ilGenerator, basicBlock, loopNode.Initialize);

            ilGenerator.MarkLabel(loopHead);
            EmitStatement(ilGenerator, basicBlock, loopNode.Body);
            if (loopNode.Step != null)
                EmitStatement(ilGenerator, basicBlock, loopNode.Step);

            ilGenerator.MarkLabel(loopCondition);
            if (loopNode.Condition != null)
            {
                ExpressionCompiler.EmitExpression(ilGenerator, loopNode.Condition, false);
                ilGenerator.Emit(OpCodes.Brtrue, loopHead);
            }

        }
    }
}
