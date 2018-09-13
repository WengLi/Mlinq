using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal sealed class SymbolTable
    {
        private readonly List<Dictionary<string, Symbol>> symbols = new List<Dictionary<string, Symbol>>();

        internal void EnterScope()
        {
            symbols.Add(new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase));
        }

        internal void ExitScope()
        {
            symbols.RemoveAt(symbols.Count - 1);
        }

        internal void Add(string name, Symbol value)
        {
            symbols[symbols.Count - 1][name] = value;
        }

        internal Symbol Lookup(string name)
        {
            for (var i = symbols.Count - 1; i >= 0; --i)
            {
                if (symbols[i].ContainsKey(name))
                {
                    return symbols[i][name];
                }
            }
            return null;
        }

    }
}
