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
    public class UncalledMethodCodeIssueProvider : ICodeIssueProvider
    {
        public IEnumerable<Type> SyntaxNodeTypes
        {
            get { yield return typeof(BaseMethodDeclarationSyntax); }
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            var model = document.GetSemanticModel(cancellationToken);
            var symbol = (MethodSymbol)model.GetDeclaredSymbol(node, cancellationToken);

            var entryPoint = model.Compilation.GetEntryPoint(cancellationToken);
            if (symbol.Equals(entryPoint))
                yield break;

            var callers = symbol.FindCallers(document.Project.Solution, cancellationToken);

            if (callers.Any())
                yield break;

            var interfaceMembers = symbol.FindImplementedInterfaceMembers(document.Project.Solution, cancellationToken);

            if (interfaceMembers.Any(im => !im.IsDeclaredInSource()))
                yield break;

            if (symbol.IsExternallyVisible())
                yield break;
            
            yield return new CodeIssue(CodeIssueKind.Unnecessary, node.Span,
                string.Format("{0} is never called.", symbol.Name),
                new RemoveMethodCodeAction(document, (BaseMethodDeclarationSyntax)node));
        }

        #region Unimplemented ICodeIssueProvider members

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> SyntaxTokenKinds
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
}
