using System.Reflection.Emit;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public static class MethodCompiler
    {
        public static void EmitMethodBody(MethodBuilder methodBuilder, MethodBodyNode node)
        {
            var ilGenerator = methodBuilder.GetILGenerator();

            foreach (var variable in node.Variables)
            {
                ilGenerator.DeclareLocal(variable.Type);
            }

            EmitStatement(ilGenerator, node.Body);
        }

        private static void EmitStatement(ILGenerator ilGenerator, AstNode node)
        {
            switch (node)
            {
                case ReturnNode returnNode: EmitReturn(ilGenerator, returnNode); break;
                case BlockNode blockNode: EmitBlock(ilGenerator, blockNode); break;
                case LoopNode loopNode: EmitLoop(ilGenerator, loopNode); break;
                default: ExpressionCompiler.EmitExpression(ilGenerator, node, true); break;
            }
        }

        private static void EmitReturn(ILGenerator ilGenerator, ReturnNode node)
        {
            if (node.Node != null)
                ExpressionCompiler.EmitExpression(ilGenerator, node.Node, false);

            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void EmitBlock(ILGenerator ilGenerator, BlockNode node)
        {
            foreach (var statement in node.Statements)
            {
                EmitStatement(ilGenerator, statement);
            }
        }

        private static void EmitLoop(ILGenerator ilGenerator, LoopNode loopNode)
        {
            var loopHead = ilGenerator.DefineLabel();
            var loopCondition = ilGenerator.DefineLabel();

            if (loopNode.Initialize != null)
                EmitStatement(ilGenerator, loopNode.Initialize);

            if (!loopNode.RunAtLeastOnce)
                ilGenerator.Emit(OpCodes.Br, loopCondition);

            ilGenerator.MarkLabel(loopHead);
            EmitStatement(ilGenerator, loopNode.Body);
            if (loopNode.Step != null)
                EmitStatement(ilGenerator, loopNode.Step);

            ilGenerator.MarkLabel(loopCondition);
            if (loopNode.Condition != null)
            {
                ExpressionCompiler.EmitExpression(ilGenerator, loopNode.Condition, false);
                ilGenerator.Emit(OpCodes.Brtrue, loopHead);
            }

        }
    }
}
