using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Services;
using Roslyn.Services.Editor;
using Roslyn.Compilers.Common;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Refactorings
{
    [ExportCodeIssueProvider("Refactorings", LanguageNames.CSharp)]
    class MutatedValueTypeArgumentCodeIssueProvider : ICodeIssueProvider
    {
        public IEnumerable<Type> SyntaxNodeTypes
        {
            get { yield return typeof(ParameterSyntax); }
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            var parameter = (ParameterSyntax)node;
            var model = document.GetSemanticModel(cancellationToken);

            var parameterSymbol = (ParameterSymbol)model.GetDeclaredSymbol(parameter, cancellationToken);
            
            if (!parameterSymbol.Type.IsValueType)
                yield break;

            if (parameterSymbol.RefKind != RefKind.None) // suppress issue when argument is ref/out
                yield break;

            var parentMethodOrLamdbaSymbol = parameterSymbol.ContainingSymbol;

            if (parentMethodOrLamdbaSymbol.IsAbstract)
                yield break;

            var methodBody = parameter.GetParentMethodBody();
            IDataFlowAnalysis dataFlow = model.AnalyzeDataFlow(methodBody);
            
            if (!dataFlow.Succeeded)
                yield break;

            if (dataFlow.WrittenInside.Contains(parameterSymbol))
                yield return new CodeIssue(CodeIssueKind.Warning, node.Span, "Do not mutate the values of value type parameters.");

        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> SyntaxTokenKinds
        {
            get { return null; }
        }
    }
}
