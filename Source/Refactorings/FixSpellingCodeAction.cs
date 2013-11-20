using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace Refactorings
{
    class FixSpellingCodeAction : ICodeAction
    {
        private readonly IDocument Document;
        private readonly CommonSyntaxNode Node;
        private readonly string OldIdentifier;
        private readonly string NewIdentifier;

        public FixSpellingCodeAction(IDocument document, CommonSyntaxNode syntaxNode, string oldIdentifier, string newIdentifier)
        {
            this.Document = document;
            this.Node = syntaxNode;
            this.OldIdentifier = oldIdentifier;
            this.NewIdentifier = newIdentifier;
        }

        public string Description
        {
            get { return string.Format("Replace {0} with {1}", OldIdentifier, NewIdentifier); }
        }

        public CodeActionEdit GetEdit(CancellationToken cancellationToken)
        {
            VariableDeclaratorSyntax variableDeclarator; // includes fields
            BaseTypeDeclarationSyntax typeDeclaration;
            MethodDeclarationSyntax methodDeclaration;
            NamespaceDeclarationSyntax namespaceDeclaration;
            EnumMemberDeclarationSyntax enumMemberDeclaration;
            PropertyDeclarationSyntax propertyDeclaration;
            EventDeclarationSyntax eventDeclaration;
            ParameterSyntax parameterSyntax;
            CommonSyntaxNode newNode;

            if ((variableDeclarator = Node as VariableDeclaratorSyntax) != null)
                newNode = variableDeclarator.WithIdentifier(Syntax.Identifier(NewIdentifier));
            else if ((parameterSyntax = Node as ParameterSyntax) != null)
                newNode = parameterSyntax.WithIdentifier(Syntax.Identifier(NewIdentifier));
            else if ((propertyDeclaration = Node as PropertyDeclarationSyntax) != null)
                newNode = propertyDeclaration.WithIdentifier(Syntax.Identifier(NewIdentifier));
            else if ((typeDeclaration = Node as BaseTypeDeclarationSyntax) != null)
                return new CodeActionEdit(Document);
                //newNode = typeDeclaration.WithIdentifier(Syntax.Identifier(NewIdentifier));
            else if ((methodDeclaration = Node as MethodDeclarationSyntax) != null)
                newNode = methodDeclaration.WithIdentifier(Syntax.Identifier(NewIdentifier));
            else if ((enumMemberDeclaration = Node as EnumMemberDeclarationSyntax) != null)
                newNode = enumMemberDeclaration.WithIdentifier(Syntax.Identifier(NewIdentifier));
            else if ((eventDeclaration = Node as EventDeclarationSyntax) != null)
                newNode = eventDeclaration.WithIdentifier(Syntax.Identifier(NewIdentifier));
            else if ((namespaceDeclaration = Node as NamespaceDeclarationSyntax) != null)
            {
                return new CodeActionEdit(Document);
                //identifiers = namespaceDeclaration.Name.ChildNodes().OfType<IdentifierNameSyntax>()
                //    .Select(i => i.Identifier).ToArray();
            }
            else
            {
                throw new ArgumentException("Node type is unrecognized.");
            }

            var newRoot = Document.GetSyntaxRoot(cancellationToken)
                .ReplaceNode(Node, newNode);

            var newDocument = Document.UpdateSyntaxRoot(newRoot);

            return new CodeActionEdit(newDocument);
        }
    }
}
