using System;

namespace HeartScript.Ast.Nodes
{
    public class UnaryNode : AstNode
    {
        public AstNode Operand { get; }

        public UnaryNode(AstNode operand, Type type, AstType nodeType) : base(type, nodeType)
        {
            Operand = operand;
        }

        public UnaryNode(AstNode operand, AstType nodeType) : this(operand, operand.Type, nodeType)
        {
        }
    }

    public partial class AstNode
    {
        public static UnaryNode UnaryPlus(AstNode operand) => new UnaryNode(operand, AstType.UnaryPlus);
        public static UnaryNode Negate(AstNode operand) => new UnaryNode(operand, AstType.Negate);
        public static UnaryNode Not(AstNode operand) => new UnaryNode(operand, AstType.Not);
        public static UnaryNode Convert(AstNode operand, Type type) => new UnaryNode(operand, type, AstType.Convert);
        public static UnaryNode Duplicate(AstNode operand) => new UnaryNode(operand, AstType.Duplicate);
        public static UnaryNode PostIncrement(AstNode operand) => new UnaryNode(operand, AstType.PostIncrement);
        public static UnaryNode PostDecrement(AstNode operand) => new UnaryNode(operand, AstType.PostDecrement);
    }
}
