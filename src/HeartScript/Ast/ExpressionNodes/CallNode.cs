using System;
using System.Reflection;

namespace HeartScript.Ast.Nodes
{
    public class CallNode : AstNode
    {
        public AstNode? Instance { get; }
        public MethodInfo MethodInfo { get; }
        public AstNode[] Parameters { get; }

        public CallNode(AstNode? instance, MethodInfo methodInfo, AstNode[] parameters) : base(methodInfo.ReturnType, AstType.Default)
        {
            if (methodInfo.IsStatic && instance != null)
                throw new ArgumentException($"{nameof(instance)} must be null for static method");

            if (!methodInfo.IsStatic && instance == null)
                throw new ArgumentException($"{nameof(instance)} must not be null for instance method");

            var parameterInfos = methodInfo.GetParameters();

            if (parameters.Length != parameterInfos.Length)
                throw new ArgumentException($"Parameter Count mismatch");

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                if (parameters[i].Type != parameterInfos[i].ParameterType)
                    throw new ArgumentException($"Parameter Type mismatch, {i}");
            }

            Instance = instance;
            MethodInfo = methodInfo;
            Parameters = parameters;
        }
    }

    public partial class AstNode
    {
        public static CallNode Call(AstNode? instance, MethodInfo methodInfo, AstNode[] parameters) => new CallNode(instance, methodInfo, parameters);
    }
}
