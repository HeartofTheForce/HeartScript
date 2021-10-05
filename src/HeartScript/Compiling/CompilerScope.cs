using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HeartScript.Compiling
{
    public class CompilerScope
    {
        private readonly Dictionary<string, Expression> _variables;

        public CompilerScope()
        {
            _variables = new Dictionary<string, Expression>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetVariable(string name, out Expression expression)
        {
            return _variables.TryGetValue(name, out expression);
        }
    }
}
