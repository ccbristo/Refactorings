using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.Common;

namespace Refactorings
{
    public static class SymbolExtensions
    {
        public static bool IsDeclaredInSource(this ISymbol symbol)
        {
            return symbol.DeclaringSyntaxNodes.Any();
        }
    }
}
