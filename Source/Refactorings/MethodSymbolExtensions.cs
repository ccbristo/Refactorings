using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace Refactorings
{
    public static class MethodSymbolExtensions
    {
        public static bool SignatureIsRequiredToSatisfyExternalDependency(this BaseMethodDeclarationSyntax method, IDocument document, CancellationToken cancellationToken)
        {
            var model = document.GetSemanticModel();
            var methodSymbol = model.GetDeclaredSymbol(method, cancellationToken);
            var entryPoint = model.Compilation.GetEntryPoint(cancellationToken);

            if (methodSymbol.Equals(entryPoint))
                return true;

            var parentMethodSymbol = model.GetDeclaredSymbol(method, cancellationToken);
            var interfaceMembers = parentMethodSymbol.FindImplementedInterfaceMembers(document.Project.Solution, cancellationToken);
            var overrides = parentMethodSymbol.FindOverrides(document.Project.Solution, cancellationToken);
            var allBaseMethods = interfaceMembers.Union(overrides);

            bool signatureIsRequired = allBaseMethods.Any(bm => !bm.IsDeclaredInSource());
            return signatureIsRequired;
        }

        public static bool IsExternallyVisible(this MethodSymbol methodSymbol)
        {
            if (methodSymbol.DeclaredAccessibility == Accessibility.Private ||
                methodSymbol.DeclaredAccessibility == Accessibility.Internal)
                return false;

            if (methodSymbol.ContainingSymbol is MethodSymbol)
                return false; // lambda (i think)

           NamedTypeSymbol typeSymbol = methodSymbol.ContainingType;
           while(typeSymbol != null)
           {
               if (typeSymbol.DeclaredAccessibility == Accessibility.Private ||
                   typeSymbol.DeclaredAccessibility == Accessibility.Internal)
                   return false;

               typeSymbol = typeSymbol.ContainingType;
           }

           return true;
        }
    }
}
