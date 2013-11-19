﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public SpellingCodeIssueProvider()
        {
            //WordLookupService service = new WordLookupService();
            //service.Initialize();
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            yield break;
        }

        static readonly Regex AlphaLongerThanTwoCharacters = new Regex(@"^\w{2,}$", RegexOptions.Compiled);

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            VariableDeclaratorSyntax variableDeclarator;
            BaseTypeDeclarationSyntax typeDeclaration;
            MethodDeclarationSyntax methodDeclaration;
            NamespaceDeclarationSyntax namespaceDeclaration;
            EnumMemberDeclarationSyntax enumMemberDeclaration;
            PropertyDeclarationSyntax propertyDeclaration;
            //FieldDeclarationSyntax fieldDeclaration; // fields are handled by VariableDeclaratorSyntax
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

            foreach (var identifier in identifiers)
            {
                var words = camelCaseSplit(identifier.ValueText)
                .Where(w => AlphaLongerThanTwoCharacters.IsMatch(w));

                foreach (var word in words)
                {
                    if (WordLookupService.SearchExact(word.ToLower()))
                        continue;

                    var matches = WordLookupService.Search(word.ToLower())
                        .Select(m => char.IsUpper(identifier.ValueText[0]) ? (char.ToUpper(m[0]) + m.Substring(1)) : m)
                        .ToList();

                    if (!matches.Contains(word, StringComparer.InvariantCultureIgnoreCase))
                    {
                        var actions = new List<ICodeAction>();

                        foreach (var suggestion in matches)
                        {
                            actions.Add(new FixSpellingCodeAction(document, node, identifier.ValueText, 
                                identifier.ValueText.Replace(word, suggestion)));
                        }

                        yield return new CodeIssue(CodeIssueKind.Warning, identifier.Span,
                                string.Format("Possible mis-spelling: {0}", word), actions);
                    }
                }
            }
        }

        public IEnumerable<Type> SyntaxNodeTypes
        {
            get
            {
                yield return typeof(VariableDeclaratorSyntax);
                yield return typeof(ParameterSyntax);
                yield return typeof(PropertyDeclarationSyntax);
                //yield return typeof(FieldDeclarationSyntax); // fields are handled by VariableDeclarator
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
