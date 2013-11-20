using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace Refactorings
{
    [ExportCodeIssueProvider("Refactorings", LanguageNames.CSharp)]
    public class SpellingCodeIssueProvider : ICodeIssueProvider
    {
        static readonly WordLookupService WordLookupService = new WordLookupService();

        private class SpellingIssue
        {
            public readonly TextSpan Span;
            public readonly string Word;
            public readonly IEnumerable<ICodeAction> Actions;

            public SpellingIssue(TextSpan span, string word, IEnumerable<ICodeAction> actions)
            {
                this.Span = span;
                this.Word = word;
                this.Actions = actions;
            }
        }

        public SpellingCodeIssueProvider()
        {
            //WordLookupService service = new WordLookupService();
            //service.Initialize();
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            yield break;
        }

        static readonly Regex AlphaLongerThanTwoCharacters = new Regex(@"^\p{L}{2,}$", RegexOptions.Compiled);

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            var identifiers = GetIdentifiers(node);

            foreach (var identifier in identifiers)
            {
                var words = camelCaseSplit(identifier.ValueText)
                            .Where(w => AlphaLongerThanTwoCharacters.IsMatch(w));

                var allIssues = new ConcurrentBag<SpellingIssue>();
                var result = Parallel.ForEach(words,
                    () => (SpellingIssue)null,
                    (word, state, dummy) => IdentifyIssues(word, document, identifier, node, state, dummy),
                    issue => allIssues.Add(issue));

                foreach (var spellingIssue in allIssues.Where(i => i != null))
                {
                    yield return new CodeIssue(CodeIssueKind.Warning, spellingIssue.Span,
                                               string.Format("Possible mis-spelling: {0}", spellingIssue.Word),
                                               spellingIssue.Actions);
                }
            }
        }

        private SpellingIssue IdentifyIssues(string word,
            IDocument document,
            SyntaxToken identifier,
            CommonSyntaxNode node,
            ParallelLoopState state,
            SpellingIssue dummy)
        {
            bool found = WordLookupService.SearchExact(word.ToLower());
            if (found)
                return (SpellingIssue)null;

            var suggestions = WordLookupService.Search(word.ToLower())
                .Select(m => char.IsUpper(identifier.ValueText[0]) ? (char.ToUpper(m[0]) + m.Substring(1)) : m)
                .ToList();

            var actions = new List<ICodeAction>();

            foreach (var suggestion in suggestions)
            {
                actions.Add(new FixSpellingCodeAction(document, node, identifier.ValueText,
                    identifier.ValueText.Replace(word, suggestion)));
            }

            var spanStart = identifier.Span.Start + identifier.ValueText.IndexOf(word, StringComparison.InvariantCultureIgnoreCase);
            var span = new TextSpan(spanStart, word.Length);
            return new SpellingIssue(span, word, actions);
        }

        private static SyntaxToken[] GetIdentifiers(CommonSyntaxNode node)
        {
            VariableDeclaratorSyntax variableDeclarator; // includes fields
            BaseTypeDeclarationSyntax typeDeclaration;
            MethodDeclarationSyntax methodDeclaration;
            NamespaceDeclarationSyntax namespaceDeclaration;
            EnumMemberDeclarationSyntax enumMemberDeclaration;
            PropertyDeclarationSyntax propertyDeclaration;
            EventDeclarationSyntax eventDeclaration;
            ParameterSyntax parameterSyntax;
            SyntaxToken[] identifiers;

            if ((variableDeclarator = node as VariableDeclaratorSyntax) != null)
                identifiers = new[] { variableDeclarator.Identifier };
            else if ((parameterSyntax = node as ParameterSyntax) != null)
                identifiers = new[] { parameterSyntax.Identifier };
            else if ((propertyDeclaration = node as PropertyDeclarationSyntax) != null)
                identifiers = new[] { propertyDeclaration.Identifier };
            else if ((typeDeclaration = node as BaseTypeDeclarationSyntax) != null)
                identifiers = new[] { typeDeclaration.Identifier };
            else if ((methodDeclaration = node as MethodDeclarationSyntax) != null)
                identifiers = new[] { methodDeclaration.Identifier };
            else if ((enumMemberDeclaration = node as EnumMemberDeclarationSyntax) != null)
                identifiers = new[] { enumMemberDeclaration.Identifier };
            else if ((eventDeclaration = node as EventDeclarationSyntax) != null)
                identifiers = new[] { eventDeclaration.Identifier };
            else if ((namespaceDeclaration = node as NamespaceDeclarationSyntax) != null)
                identifiers = namespaceDeclaration.Name.ChildNodes().OfType<IdentifierNameSyntax>()
                    .Select(i => i.Identifier).ToArray();
            else
            {
                throw new ArgumentException("Unhandled node.");
            }
            return identifiers;
        }

        public IEnumerable<Type> SyntaxNodeTypes
        {
            get
            {
                yield return typeof(VariableDeclaratorSyntax); // includes fields
                yield return typeof(ParameterSyntax);
                yield return typeof(PropertyDeclarationSyntax);
                yield return typeof(MethodDeclarationSyntax);
                yield return typeof(EnumMemberDeclarationSyntax);
                yield return typeof(NamespaceDeclarationSyntax);
                yield return typeof(BaseTypeDeclarationSyntax);
                yield return typeof(EventDeclarationSyntax);
            }
        }

        private static readonly Regex NonLowerFollowedByUpperLower = new Regex(@"(\P{Ll})\B(\P{Ll}\p{Ll})", RegexOptions.Compiled);
        private static readonly Regex LowerCaseFollowedByNonLower = new Regex(@"(\p{Ll})\B(\P{Ll})", RegexOptions.Compiled);

        private string[] camelCaseSplit(string name)
        {
            string noUnderscores = name.Replace("_", " ");
            string spacesAddedIn_AAb = NonLowerFollowedByUpperLower.Replace(noUnderscores, "$1 $2");
            string spacesAddedIn_aB = LowerCaseFollowedByNonLower.Replace(spacesAddedIn_AAb, "$1 $2");
            return spacesAddedIn_aB.Split(' ');
        }

        public IEnumerable<int> SyntaxTokenKinds
        {
            get { return null; }
        }
    }
}
