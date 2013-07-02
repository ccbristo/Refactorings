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
            return symbol.DeclaringSyntaxNodes != null && symbol.DeclaringSyntaxNodes.Any();
        }

        public static bool IsExternallyVisible(this ISymbol symbol)
        {
            if (symbol.DeclaredAccessibility == CommonAccessibility.Private ||
                symbol.DeclaredAccessibility == CommonAccessibility.Internal)
                return false;

            INamedTypeSymbol containingType = symbol.ContainingType;
            while (containingType != null)
            {
                if (containingType.DeclaredAccessibility == CommonAccessibility.Private ||
                    containingType.DeclaredAccessibility == CommonAccessibility.Internal)
                    return false;

                containingType = containingType.ContainingType;
            }

            return true;
        }
    }
}
