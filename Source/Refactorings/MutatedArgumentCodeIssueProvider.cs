using System;
using System.Collections.Generic;
using System.Threading;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;

namespace Refactorings
{
    [ExportCodeIssueProvider("Refactorings", LanguageNames.CSharp)]
    class MutatedArgumentCodeIssueProvider : ICodeIssueProvider
    {
        public IEnumerable<Type> SyntaxNodeTypes
        {
            get
            {
                yield return typeof(ExpressionStatementSyntax);
                yield return typeof(PostfixUnaryExpressionSyntax);
                yield return typeof(PrefixUnaryExpressionSyntax);
            }
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            ExpressionStatementSyntax expressionStatement;
            PostfixUnaryExpressionSyntax postFixExpression;
            PrefixUnaryExpressionSyntax prefixExpression;
            ISymbol mutatedSymbol;

            if ((expressionStatement = node as ExpressionStatementSyntax) != null)
                mutatedSymbol = GetMutatedSymbol(document, cancellationToken, expressionStatement);
            else if ((postFixExpression = node as PostfixUnaryExpressionSyntax) != null)
                mutatedSymbol = GetMutatedSymbol(document, cancellationToken, postFixExpression);
            else if ((prefixExpression = node as PrefixUnaryExpressionSyntax) != null)
                mutatedSymbol = GetMutatedSymbol(document, cancellationToken, prefixExpression);
            else
                throw new ArgumentException("Unhandled node type.");

            if (mutatedSymbol != null && mutatedSymbol.Kind == CommonSymbolKind.Parameter &&
                ((ParameterSymbol)mutatedSymbol).RefKind == RefKind.None)
                yield return new CodeIssue(CodeIssueKind.Warning, node.Span, "Do not mutate the values of parameters.");
        }

        private ISymbol GetMutatedSymbol(IDocument document, CancellationToken cancellationToken, ExpressionStatementSyntax expressionStatement)
        {
            if (expressionStatement.Expression.Kind != SyntaxKind.AssignExpression &&
                expressionStatement.Expression.Kind != SyntaxKind.AddAssignExpression &&
                expressionStatement.Expression.Kind != SyntaxKind.DivideAssignExpression &&
                expressionStatement.Expression.Kind != SyntaxKind.MultiplyAssignExpression &&
                expressionStatement.Expression.Kind != SyntaxKind.SubtractAssignExpression &&
                expressionStatement.Expression.Kind != SyntaxKind.ModuloAssignExpression &&
                expressionStatement.Expression.Kind != SyntaxKind.AndAssignExpression &&
                expressionStatement.Expression.Kind != SyntaxKind.OrAssignExpression)
                return null;

            var model = document.GetSemanticModel(cancellationToken);
            var binaryExpression = (BinaryExpressionSyntax)expressionStatement.Expression;

            var symbolInfo = model.GetSymbolInfo(binaryExpression.Left);
            return symbolInfo.Symbol;
        }

        private ISymbol GetMutatedSymbol(IDocument document, CancellationToken cancellationToken, PostfixUnaryExpressionSyntax postFixExpression)
        {
            var model = document.GetSemanticModel(cancellationToken);
            var symbolInfo = model.GetSymbolInfo(postFixExpression.Operand, cancellationToken);
            return symbolInfo.Symbol;
        }

        private ISymbol GetMutatedSymbol(IDocument document, CancellationToken cancellationToken, PrefixUnaryExpressionSyntax prefixExpression)
        {
            var model = document.GetSemanticModel(cancellationToken);
            var symbolInfo = model.GetSymbolInfo(prefixExpression.Operand, cancellationToken);
            return symbolInfo.Symbol;
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
