using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers.CSharp;

namespace Refactorings
{
    public static class ParameterSyntaxExtensions
    {
        public static SyntaxNode GetParentMethodBody(this ParameterSyntax parameter)
        {
            var parentMethod = parameter.Ancestors().FirstOrDefault(n => n is SimpleLambdaExpressionSyntax ||
                n is BaseMethodDeclarationSyntax ||
                n is ParenthesizedLambdaExpressionSyntax);

            if (parentMethod is BaseMethodDeclarationSyntax)
                return ((BaseMethodDeclarationSyntax)parentMethod).Body;
            else if (parentMethod is SimpleLambdaExpressionSyntax)
                return ((SimpleLambdaExpressionSyntax)parentMethod).Body;
            else if (parentMethod is ParenthesizedLambdaExpressionSyntax)
                return ((ParenthesizedLambdaExpressionSyntax)parentMethod).Body;
            else
                throw new InvalidOperationException("Unhandled expression syntax.");
        }
    }
}
