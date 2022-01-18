using System.Collections.Generic;
using System.Reflection.Emit;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public static class MethodCompiler
    {
        private class MethodBodyContext
        {
            public Label ReturnLabel { get; set; }
            public LocalBuilder? ReturnLocal { get; set; }
            public Stack<Label> BreakLabel { get; set; }
            public Stack<Label> ContinueLabel { get; set; }

            public MethodBodyContext(Label returnLabel, LocalBuilder? returnLocal)
            {
                ReturnLabel = returnLabel;
                ReturnLocal = returnLocal;
                BreakLabel = new Stack<Label>();
                ContinueLabel = new Stack<Label>();
            }
        }

        public static void EmitMethodBody(MethodBuilder methodBuilder, MethodBodyNode node)
        {
            var ilGenerator = methodBuilder.GetILGenerator();

            foreach (var variable in node.Variables)
            {
                ilGenerator.DeclareLocal(variable.Type);
            }

            var ctx = new MethodBodyContext(
                ilGenerator.DefineLabel(),
                methodBuilder.ReturnType != typeof(void) ? ilGenerator.DeclareLocal(methodBuilder.ReturnType) : null);

            EmitStatement(ilGenerator, ctx, node.Body);

            ilGenerator.MarkLabel(ctx.ReturnLabel);
            if (ctx.ReturnLocal != null)
                ilGenerator.Emit(OpCodes.Ldloc, ctx.ReturnLocal);

            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void EmitStatement(ILGenerator ilGenerator, MethodBodyContext ctx, AstNode node)
        {
            switch (node)
            {
                case ReturnNode returnNode: EmitReturn(ilGenerator, ctx, returnNode); break;
                case BlockNode blockNode: EmitBlock(ilGenerator, ctx, blockNode); break;
                case LoopNode loopNode: EmitLoop(ilGenerator, ctx, loopNode); break;
                case IfElseNode ifElseNode: EmitIfElse(ilGenerator, ctx, ifElseNode); break;
                case BreakNode _: ilGenerator.Emit(OpCodes.Br, ctx.BreakLabel.Peek()); break;
                case ContinueNode _: ilGenerator.Emit(OpCodes.Br, ctx.ContinueLabel.Peek()); break;
                default: ExpressionCompiler.EmitExpression(ilGenerator, node, true); break;
            }
        }

        private static void EmitReturn(ILGenerator ilGenerator, MethodBodyContext ctx, ReturnNode node)
        {
            if (node.Node != null)
            {
                ExpressionCompiler.EmitExpression(ilGenerator, node.Node, false);
                ilGenerator.Emit(OpCodes.Stloc, ctx.ReturnLocal);
            }

            ilGenerator.Emit(OpCodes.Br, ctx.ReturnLabel);
        }

        private static void EmitBlock(ILGenerator ilGenerator, MethodBodyContext ctx, BlockNode node)
        {
            foreach (var statement in node.Statements)
            {
                EmitStatement(ilGenerator, ctx, statement);
            }
        }

        private static void EmitLoop(ILGenerator ilGenerator, MethodBodyContext ctx, LoopNode loopNode)
        {
            var loopHead = ilGenerator.DefineLabel();
            var loopCondition = ilGenerator.DefineLabel();
            var loopStep = ilGenerator.DefineLabel();
            var loopTail = ilGenerator.DefineLabel();

            ctx.BreakLabel.Push(loopTail);
            ctx.ContinueLabel.Push(loopStep);

            if (loopNode.Initialize != null)
                EmitStatement(ilGenerator, ctx, loopNode.Initialize);

            if (!loopNode.RunAtLeastOnce)
                ilGenerator.Emit(OpCodes.Br, loopCondition);

            ilGenerator.MarkLabel(loopHead);
            EmitStatement(ilGenerator, ctx, loopNode.Body);

            ilGenerator.MarkLabel(loopStep);
            if (loopNode.Step != null)
                EmitStatement(ilGenerator, ctx, loopNode.Step);

            ilGenerator.MarkLabel(loopCondition);
            if (loopNode.Condition != null)
            {
                ExpressionCompiler.EmitExpression(ilGenerator, loopNode.Condition, false);
                ilGenerator.Emit(OpCodes.Brtrue, loopHead);
            }

            ilGenerator.MarkLabel(loopTail);

            ctx.BreakLabel.Pop();
            ctx.ContinueLabel.Pop();
        }

        private static void EmitIfElse(ILGenerator ilGenerator, MethodBodyContext ctx, IfElseNode node)
        {
            if (node.IfFalse != null)
            {
                var ifFalseLabel = ilGenerator.DefineLabel();
                var endLabel = ilGenerator.DefineLabel();

                ExpressionCompiler.EmitExpression(ilGenerator, node.Condition, false);
                ilGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

                //true
                EmitStatement(ilGenerator, ctx, node.IfTrue);
                ilGenerator.Emit(OpCodes.Br, endLabel);

                //false
                ilGenerator.MarkLabel(ifFalseLabel);
                EmitStatement(ilGenerator, ctx, node.IfFalse);

                ilGenerator.MarkLabel(endLabel);
            }
            else
            {
                var endLabel = ilGenerator.DefineLabel();
                ExpressionCompiler.EmitExpression(ilGenerator, node.Condition, false);
                ilGenerator.Emit(OpCodes.Brfalse, endLabel);

                //true
                EmitStatement(ilGenerator, ctx, node.IfTrue);

                ilGenerator.MarkLabel(endLabel);
            }
        }
    }
}
