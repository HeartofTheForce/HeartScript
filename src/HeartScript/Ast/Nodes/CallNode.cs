using System;
using System.Reflection;

namespace HeartScript.Ast.Nodes
{
    public class CallNode : AstNode
    {
        public AstNode Target { get; }
        public AstNode[] Parameters { get; }

        public CallNode(MethodInfo methodInfo, AstNode target, AstNode[] parameters) : base(methodInfo.ReturnType, AstType.Call)
        {
            var parameterInfos = methodInfo.GetParameters();

            if (parameters.Length != parameterInfos.Length)
                throw new ArgumentException($"Parameter Count mismatch");

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                if (parameters[i].Type != parameterInfos[i].ParameterType)
                    throw new ArgumentException($"Parameter Type mismatch, {i}");
            }

            Target = target;
            Parameters = parameters;
        }
    }
}
