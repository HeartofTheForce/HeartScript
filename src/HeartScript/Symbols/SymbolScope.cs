using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HeartScript.Ast
{
    public class SymbolScope
    {
        private readonly SymbolScope? _parent;
        private readonly Dictionary<string, ISymbol> _symbols;

        public SymbolScope(SymbolScope? parent)
        {
            _parent = parent;
            _symbols = new Dictionary<string, ISymbol>();
        }

        public SymbolScope() : this(null)
        {
        }

        public bool TryGetSymbol(string name, [NotNullWhen(true)] out ISymbol? symbol)
        {
            symbol = null;
            var current = this;
            while (current != null)
            {
                if (current._symbols.TryGetValue(name, out var candidate))
                {
                    if (candidate.IsPublic || current == this)
                    {
                        symbol = candidate;
                        break;
                    }
                }

                current = current._parent;
            }

            if (symbol != null)
                return true;

            return false;
        }

        public bool TryGetSymbol<T>(string name, [NotNullWhen(true)] out ISymbol<T>? symbol)
        {
            if (TryGetSymbol(name, out var untyped) && untyped is ISymbol<T> typed)
            {
                symbol = typed;
                return true;
            }

            symbol = null;
            return false;
        }

        public void DeclareSymbol(string name, ISymbol symbol)
        {
            if (_symbols.ContainsKey(name))
                throw new ArgumentException(nameof(name));

            _symbols[name] = symbol;
        }

        public void DeclareSymbol<T>(string name, bool isPublic, T value)
        {
            if (_symbols.ContainsKey(name))
                throw new ArgumentException(nameof(name));

            _symbols[name] = new Symbol<T>(isPublic, value);
        }
    }
}
