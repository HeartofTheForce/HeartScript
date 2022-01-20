using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HeartScript.Ast.Nodes;

namespace HeartScript.Compiling.Emit
{
    public class MethodBodyContext
    {
        public ILGenerator ILGenerator { get; }
        public Label ReturnLabel { get; }
        public LocalBuilder? ReturnLocal { get; }
        public Stack<Label> BreakLabel { get; }
        public Stack<Label> ContinueLabel { get; }
        private Dictionary<Type, LocalBuilder> TempVariables { get; }

        public MethodBodyContext(ILGenerator iLGenerator, LocalBuilder? returnLocal)
        {
            ILGenerator = iLGenerator;
            ReturnLabel = iLGenerator.DefineLabel();
            ReturnLocal = returnLocal;
            BreakLabel = new Stack<Label>();
            ContinueLabel = new Stack<Label>();
            TempVariables = new Dictionary<Type, LocalBuilder>();
        }

        public LocalBuilder GetTempLocal(Type type)
        {
            if (TempVariables.TryGetValue(type, out var localBuilder))
                return localBuilder;

            localBuilder = ILGenerator.DeclareLocal(type);
            TempVariables[type] = localBuilder;

            return localBuilder;
        }
    }

    public static class MethodCompiler
    {
        public static void EmitMethodBody(MethodBuilder methodBuilder, MethodBodyNode node)
        {
            var ilGenerator = methodBuilder.GetILGenerator();
            foreach (var variable in node.Variables)
            {
                ilGenerator.DeclareLocal(variable.Type);
            }

            var ctx = new MethodBodyContext(
                ilGenerator,
                methodBuilder.ReturnType != typeof(void) ? ilGenerator.DeclareLocal(methodBuilder.ReturnType) : null);

            EmitStatement(ctx, node.Body);

            ctx.ILGenerator.MarkLabel(ctx.ReturnLabel);
            if (ctx.ReturnLocal != null)
                ctx.ILGenerator.Emit(OpCodes.Ldloc, ctx.ReturnLocal);

            ctx.ILGenerator.Emit(OpCodes.Ret);
        }

        private static void EmitStatement(MethodBodyContext ctx, AstNode node)
        {
            switch (node)
            {
                case ReturnNode returnNode: EmitReturn(ctx, returnNode); break;
                case BlockNode blockNode: EmitBlock(ctx, blockNode); break;
                case LoopNode loopNode: EmitLoop(ctx, loopNode); break;
                case IfElseNode ifElseNode: EmitIfElse(ctx, ifElseNode); break;
                case BreakNode _: ctx.ILGenerator.Emit(OpCodes.Br, ctx.BreakLabel.Peek()); break;
                case ContinueNode _: ctx.ILGenerator.Emit(OpCodes.Br, ctx.ContinueLabel.Peek()); break;
                default: ExpressionCompiler.EmitExpression(ctx, node, true); break;
            }
        }

        private static void EmitReturn(MethodBodyContext ctx, ReturnNode node)
        {
            if (node.Node != null)
            {
                ExpressionCompiler.EmitExpression(ctx, node.Node, false);
                ctx.ILGenerator.Emit(OpCodes.Stloc, ctx.ReturnLocal);
            }

            ctx.ILGenerator.Emit(OpCodes.Br, ctx.ReturnLabel);
        }

        private static void EmitBlock(MethodBodyContext ctx, BlockNode node)
        {
            foreach (var statement in node.Statements)
            {
                EmitStatement(ctx, statement);
            }
        }

        private static void EmitLoop(MethodBodyContext ctx, LoopNode loopNode)
        {
            var loopHead = ctx.ILGenerator.DefineLabel();
            var loopCondition = ctx.ILGenerator.DefineLabel();
            var loopStep = ctx.ILGenerator.DefineLabel();
            var loopTail = ctx.ILGenerator.DefineLabel();

            ctx.BreakLabel.Push(loopTail);
            ctx.ContinueLabel.Push(loopStep);

            if (loopNode.Initialize != null)
                EmitStatement(ctx, loopNode.Initialize);

            if (!loopNode.RunAtLeastOnce)
                ctx.ILGenerator.Emit(OpCodes.Br, loopCondition);

            ctx.ILGenerator.MarkLabel(loopHead);
            EmitStatement(ctx, loopNode.Body);

            ctx.ILGenerator.MarkLabel(loopStep);
            if (loopNode.Step != null)
                EmitStatement(ctx, loopNode.Step);

            ctx.ILGenerator.MarkLabel(loopCondition);
            if (loopNode.Condition != null)
            {
                ExpressionCompiler.EmitExpression(ctx, loopNode.Condition, false);
                ctx.ILGenerator.Emit(OpCodes.Brtrue, loopHead);
            }

            ctx.ILGenerator.MarkLabel(loopTail);

            ctx.BreakLabel.Pop();
            ctx.ContinueLabel.Pop();
        }

        private static void EmitIfElse(MethodBodyContext ctx, IfElseNode node)
        {
            if (node.IfFalse != null)
            {
                var ifFalseLabel = ctx.ILGenerator.DefineLabel();
                var endLabel = ctx.ILGenerator.DefineLabel();

                ExpressionCompiler.EmitExpression(ctx, node.Condition, false);
                ctx.ILGenerator.Emit(OpCodes.Brfalse, ifFalseLabel);

                //true
                EmitStatement(ctx, node.IfTrue);
                ctx.ILGenerator.Emit(OpCodes.Br, endLabel);

                //false
                ctx.ILGenerator.MarkLabel(ifFalseLabel);
                EmitStatement(ctx, node.IfFalse);

                ctx.ILGenerator.MarkLabel(endLabel);
            }
            else
            {
                var endLabel = ctx.ILGenerator.DefineLabel();
                ExpressionCompiler.EmitExpression(ctx, node.Condition, false);
                ctx.ILGenerator.Emit(OpCodes.Brfalse, endLabel);

                //true
                EmitStatement(ctx, node.IfTrue);

                ctx.ILGenerator.MarkLabel(endLabel);
            }
        }
    }
}
