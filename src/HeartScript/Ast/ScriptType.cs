using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HeartScript.Ast
{
    public class ScriptType
    {
        public string Name { get; }
        public Dictionary<string, List<ScriptMethod>> MethodGroups { get; }

        public ScriptType(Type type)
        {
            Name = type.FullName;
            MethodGroups = new Dictionary<string, List<ScriptMethod>>();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                AddMethod(new ScriptMethod(method));
            }
        }

        public ScriptType(TypeBuilder typeBuilder)
        {
            Name = typeBuilder.FullName;
            MethodGroups = new Dictionary<string, List<ScriptMethod>>();
        }

        public void AddMethod(ScriptMethod method)
        {
            if (MethodGroups.TryGetValue(method.MethodInfo.Name, out var methodGroup))
            {
                if (methodGroup.Any(existingMethod => ScriptMethod.SignaturesMatch(method, existingMethod)))
                    throw new Exception("A method with a matching signature already exists.");

                methodGroup.Add(method);
            }
            else
            {
                methodGroup = new List<ScriptMethod>() { method };
                MethodGroups[method.MethodInfo.Name] = methodGroup;
            }
        }

        public ScriptMethod? GetMethod(string methodName, BindingFlags bindingAttr, Type[] parameterTypes)
        {
            if (MethodGroups.TryGetValue(methodName, out var methodGroup))
            {
                int bestMatchQuality = 0;
                ScriptMethod? bestMatch = null;
                foreach (var method in methodGroup)
                {
                    if (parameterTypes.Length != method.ParameterTypes.Length)
                        continue;

                    bool matchBindingFlags =
                        (bindingAttr.HasFlag(BindingFlags.Public) && method.MethodInfo.IsPublic ||
                        bindingAttr.HasFlag(BindingFlags.NonPublic) && method.MethodInfo.IsPrivate) &&
                        (bindingAttr.HasFlag(BindingFlags.Static) && method.MethodInfo.IsStatic ||
                        bindingAttr.HasFlag(BindingFlags.Instance) && !method.MethodInfo.IsStatic);

                    if (!matchBindingFlags)
                        continue;

                    int matchQuality = 0;
                    bool isValid = true;
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (parameterTypes[i] == method.ParameterTypes[i])
                            matchQuality++;
                        else if (!IsConvertible(parameterTypes[i], method.ParameterTypes[i]))
                            isValid = false;
                    }

                    if (isValid)
                    {
                        if (bestMatch != null && matchQuality == bestMatchQuality)
                        {
                            throw new Exception($"The call is ambiguous between the following methods: '{bestMatch}' and '{method}'");
                        }
                        else if (matchQuality >= bestMatchQuality)
                        {
                            bestMatchQuality = matchQuality;
                            bestMatch = method;
                        }
                    }
                }

                return bestMatch;
            }

            return null;
        }

        private bool IsConvertible(Type a, Type b)
        {
            if (a == typeof(int) && b == typeof(double))
                return true;

            return false;
        }
    }
}
