using System;
using System.Linq;
using System.Reflection;

namespace HeartScript.Ast
{
    public class ScriptMethod
    {
        public MethodInfo MethodInfo { get; }
        public Type[] ParameterTypes { get; }

        public ScriptMethod(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            ParameterTypes = methodInfo
                .GetParameters()
                .Select(X => X.ParameterType)
                .ToArray();
        }

        public ScriptMethod(MethodInfo methodInfo, Type[] parameters)
        {
            MethodInfo = methodInfo;
            ParameterTypes = parameters;
        }

        public static bool SignaturesMatch(ScriptMethod a, ScriptMethod b)
        {
            if (a.MethodInfo.Name != b.MethodInfo.Name)
                return false;

            if (a.ParameterTypes.Length != b.ParameterTypes.Length)
                return false;

            for (int i = 0; i < a.ParameterTypes.Length; i++)
            {
                if (a.ParameterTypes[i] != b.ParameterTypes[i])
                    return false;
            }

            return true;
        }
    }
}
