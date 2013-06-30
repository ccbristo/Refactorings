using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace Refactorings
{
    [ExportCodeIssueProvider("Refactorings", LanguageNames.CSharp)]
    class UnusedParameterCodeIssueProvider : ICodeIssueProvider
    {
        public IEnumerable<Type> SyntaxNodeTypes
        {
            get { yield return typeof(ParameterSyntax); }
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            var parameterSyntax = (ParameterSyntax)node;
            var parentMethodBody = parameterSyntax.GetParentMethodBody();
            
            if (parentMethodBody == null) // abstract/interface/extern
                yield break;

            var model = document.GetSemanticModel(cancellationToken);
            var parentMethod = parameterSyntax.Parent.Parent as BaseMethodDeclarationSyntax;

            if (parentMethod != null &&
                parentMethod.SignatureIsRequiredToSatisfyExternalDependency(document, cancellationToken))
                yield break;

            var dataFlow = model.AnalyzeDataFlow(parentMethodBody);

            if (!dataFlow.Succeeded)
                yield break;

            var parameterSymbol = model.GetDeclaredSymbol(node, cancellationToken);

            bool isUsed = dataFlow.ReadInside.Contains(parameterSymbol) || dataFlow.WrittenInside.Contains(parameterSymbol);

            if (!isUsed)
                yield return new CodeIssue(CodeIssueKind.Unnecessary, node.Span,
                    string.Format("{0} is not used.", parameterSymbol.Name));
        }

        public IEnumerable<int> SyntaxTokenKinds
        {
            get { return null; }
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
