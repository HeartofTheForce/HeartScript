using System;
using System.Reflection;

namespace HeartScript.Ast.Nodes
{
    public class CallNode : AstNode
    {
        public AstNode? Instance { get; }
        public MethodInfo MethodInfo { get; }
        public AstNode[] Parameters { get; }

        public CallNode(AstNode? instance, ScriptMethod scriptMethod, AstNode[] parameters) : base(scriptMethod.MethodInfo.ReturnType, AstType.Default)
        {
            if (scriptMethod.MethodInfo.IsStatic && instance != null)
                throw new ArgumentException($"{nameof(instance)} must be null for static method");

            if (!scriptMethod.MethodInfo.IsStatic && instance == null)
                throw new ArgumentException($"{nameof(instance)} must not be null for instance method");

            if (parameters.Length != scriptMethod.ParameterTypes.Length)
                throw new ArgumentException($"Parameter Count mismatch");

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Type != scriptMethod.ParameterTypes[i])
                    throw new ArgumentException($"Parameter Type mismatch, {i}");
            }

            Instance = instance;
            MethodInfo = scriptMethod.MethodInfo;
            Parameters = parameters;
        }
    }

    public partial class AstNode
    {
        public static CallNode Call(AstNode? instance, ScriptMethod scriptMethod, AstNode[] parameters) => new CallNode(instance, scriptMethod, parameters);
    }
}
