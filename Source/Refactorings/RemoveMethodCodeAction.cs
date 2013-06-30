using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace Refactorings
{
    class RemoveMethodCodeAction : ICodeAction
    {
        private readonly IDocument Document;
        private readonly BaseMethodDeclarationSyntax MethodDeclaration;
        private readonly string MemberType;

        public RemoveMethodCodeAction(IDocument document, BaseMethodDeclarationSyntax methodDeclaration)
        {
            this.Document = document;
            this.MethodDeclaration = methodDeclaration;
            this.MemberType = methodDeclaration is MethodDeclarationSyntax ? "method" : "constructor";
        }

        public string Description
        {
            get { return "Remove unused method/constructor"; }
        }

        public CodeActionEdit GetEdit(CancellationToken cancellationToken)
        {
            var oldRoot = Document.GetSyntaxRoot(cancellationToken);
            var newRoot = oldRoot.RemoveNode(MethodDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
            var newDocument = Document.UpdateSyntaxRoot(newRoot);
            return new CodeActionEdit(newDocument);
        }
    }
}
