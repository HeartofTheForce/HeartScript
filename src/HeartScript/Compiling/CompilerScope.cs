using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace HeartScript.Compiling
{
    public class CompilerScope
    {
        private readonly Dictionary<string, Expression> _variables;

        private CompilerScope(Dictionary<string, Expression> variables)
        {
            _variables = variables;
        }

        public static CompilerScope Empty()
        {
            var variables = new Dictionary<string, Expression>();
            return new CompilerScope(variables);
        }

        public static CompilerScope FromMembers(Expression expression)
        {
            var variables = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);

            var propertyInfos = expression.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                variables[propertyInfo.Name] = Expression.Property(expression, propertyInfo);
            }

            var fieldInfos = expression.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fieldInfos)
            {
                variables[fieldInfo.Name] = Expression.Field(expression, fieldInfo);
            }

            return new CompilerScope(variables);
        }

        public bool TryGetVariable(string name, out Expression expression)
        {
            return _variables.TryGetValue(name, out expression);
        }
    }
}
