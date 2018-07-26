using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class TriviaSummaryMapper
    {
        //public static string Map(CSharpSyntaxNode member)
        //{
        //    var leadingTrivia = member.GetLeadingTrivia();
        //    var commentTrivia = leadingTrivia.FirstOrDefault(t => t.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia);
        //    var nodes = commentTrivia.GetStructure().ChildNodes();
        //    var xmlElement = nodes.FirstOrDefault(n => n.Kind() == SyntaxKind.XmlElement && (n as XmlElementSyntax).StartTag.Name.LocalName.ValueText.Equals("summary"));
        //    var summaryContent = (xmlElement as XmlElementSyntax).Content;
        //    var summaryText = summaryContent.ToString().Replace("///", string.Empty).Trim();
        //    return summaryText;
        //}

        public static string Map(SyntaxNode member)
        {
            var commentTrivia = GetCommentTrivia(member);
            var summaryContent = GetCommentContent(commentTrivia, "summary");
            return summaryContent.Replace("'''", string.Empty).Replace("///", string.Empty).Trim();
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
