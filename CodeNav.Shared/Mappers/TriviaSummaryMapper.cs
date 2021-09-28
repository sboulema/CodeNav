using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class TriviaSummaryMapper
    {
        public static string Map(SyntaxNode member)
        {
            try
            {
                var commentTrivia = GetCommentTrivia(member);
                var summaryContent = GetCommentContent(commentTrivia, "summary");
                return summaryContent.Replace("'''", string.Empty).Replace("///", string.Empty).Trim();
            }
            catch (InvalidOperationException)
            {
                // Ignore Unexpected false
            }

            return string.Empty;
        }

        public static bool HasSummary(SyntaxNode member)
        {
            var commentTrivia = GetCommentTrivia(member);
            return commentTrivia.RawKind != (int)SyntaxKind.None;
        }

        private static SyntaxTrivia GetCommentTrivia(SyntaxNode member)
        {
            var leadingTrivia = member.GetLeadingTrivia();
            return leadingTrivia.FirstOrDefault(t => t.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia ||
                                                     t.RawKind == (int)VisualBasic.SyntaxKind.DocumentationCommentTrivia);
        }

        private static string GetCommentContent(SyntaxTrivia commentTrivia, string commentName)
        {
            var nodes = commentTrivia.GetStructure().ChildNodes();
            var xmlElement = nodes.FirstOrDefault(n => (
                                                        n.RawKind == (int)SyntaxKind.XmlElement ||
                                                        n.RawKind == (int)VisualBasic.SyntaxKind.XmlElement
                                                       ) 
                                                       && IsCommentNamed(n, commentName));
            if (xmlElement is XmlElementSyntax)
            {
                return (xmlElement as XmlElementSyntax).Content.ToString();
            }

            if (xmlElement is VisualBasicSyntax.XmlElementSyntax)
            {
                return (xmlElement as VisualBasicSyntax.XmlElementSyntax).Content.ToString();
            }

            return string.Empty;
        }

        private static bool IsCommentNamed(SyntaxNode node, string commentName)
        {
            if (node is XmlElementSyntax)
            {
                return (node as XmlElementSyntax).StartTag.Name.LocalName.ValueText.Equals(commentName);
            }

            if (node is VisualBasicSyntax.XmlElementSyntax)
            {
                return (node as VisualBasicSyntax.XmlElementSyntax).StartTag.Name.ToString().Equals(commentName);
            }

            return false;
        }
    }
}
