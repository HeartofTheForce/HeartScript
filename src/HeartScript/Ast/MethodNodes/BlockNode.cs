using System;

namespace HeartScript.Ast.Nodes
{
    public class BlockNode : AstNode
    {
        public AstNode[] Nodes { get; }

        public BlockNode(AstNode[] nodes, Type returnType) : base(returnType, AstType.Default)
        {
            Nodes = nodes;
            foreach (var node in Nodes)
            {
                if (node is ReturnNode returnNode && returnNode.Type != returnType)
                    throw new ArgumentException($"{nameof(returnNode)} type mismatch");
            }
        }
    }

    public partial class AstNode
    {
        public static BlockNode Block(AstNode[] nodes, Type returnType) => new BlockNode(nodes, returnType);
    }
}
