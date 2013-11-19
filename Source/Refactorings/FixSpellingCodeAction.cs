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
            // TODO [ccb] Implement this
            return new CodeActionEdit(Document);
        }
    }
}
