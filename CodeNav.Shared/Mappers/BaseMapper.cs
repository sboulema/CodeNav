using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Windows.Media;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class BaseMapper
    {
        public static T MapBase<T>(SyntaxNode source, SyntaxToken identifier, SyntaxTokenList modifiers, ICodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, identifier.Text, modifiers, control, semanticModel);
        }

        public static T MapBase<T>(SyntaxNode source, NameSyntax name, ICodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, name.ToString(), new SyntaxTokenList(), control, semanticModel);
        }

        public static T MapBase<T>(SyntaxNode source, VisualBasicSyntax.NameSyntax name, ICodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, name.ToString(), new SyntaxTokenList(), control, semanticModel);
        }

        public static T MapBase<T>(SyntaxNode source, string name, ICodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, name, new SyntaxTokenList(), control, semanticModel);
        }

        public static T MapBase<T>(SyntaxNode source, SyntaxToken identifier, ICodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, identifier.Text, new SyntaxTokenList(), control, semanticModel);
        }

        private static T MapBase<T>(SyntaxNode source, string name, SyntaxTokenList modifiers, ICodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            element.Name = name;
            element.FullName = GetFullName(source, name, semanticModel);
            element.Id = element.FullName;
            element.Tooltip = name;
            element.StartLine = GetStartLine(source);
            element.StartLinePosition = GetStartLinePosition(source);
            element.EndLine = GetEndLine(source);
            element.EndLinePosition = GetEndLinePosition(source);
            element.Span = source.Span;
            element.ForegroundColor = Colors.Black;
            element.Access = MapAccess(modifiers, source);
            element.FontSize = General.Instance.Font.SizeInPoints;
            element.ParameterFontSize = General.Instance.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(General.Instance.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(General.Instance.Font.Style);
            element.Control = control;

            return element;
        }

        private static string GetFullName(SyntaxNode source, string name, SemanticModel semanticModel)
        {
            try
            {
                var symbol = semanticModel.GetDeclaredSymbol(source);
                return symbol?.ToString() ?? name;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static LinePosition GetStartLinePosition(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).StartLinePosition;

        private static LinePosition GetEndLinePosition(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).EndLinePosition;

        private static int GetStartLine(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).StartLinePosition.Line + 1;

        private static int GetEndLine(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).EndLinePosition.Line + 1;

        private static CodeItemAccessEnum MapAccess(SyntaxTokenList modifiers, SyntaxNode source)
        {
            if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.SealedKeyword ||
                                   m.RawKind == (int)VisualBasic.SyntaxKind.NotOverridableKeyword))
            {
                return CodeItemAccessEnum.Sealed;
            }
            if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.PublicKeyword ||
                                   m.RawKind == (int)VisualBasic.SyntaxKind.PublicKeyword))
            {
                return CodeItemAccessEnum.Public;
            }
            if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.PrivateKeyword ||
                                   m.RawKind == (int)VisualBasic.SyntaxKind.PrivateKeyword))
            {
                return CodeItemAccessEnum.Private;
            }
            if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.ProtectedKeyword ||
                                   m.RawKind == (int)VisualBasic.SyntaxKind.ProtectedKeyword))
            {
                return CodeItemAccessEnum.Protected;
            }
            if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.InternalKeyword ||
                                   m.RawKind == (int)VisualBasic.SyntaxKind.FriendKeyword))
            {
                return CodeItemAccessEnum.Internal;
            }

            return MapDefaultAccess(source);
        }

        /// <summary>
        /// When no access modifier is given map to the default access modifier
        /// https://stackoverflow.com/questions/2521459/what-are-the-default-access-modifiers-in-c
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static CodeItemAccessEnum MapDefaultAccess(SyntaxNode source)
        {
            if (source.Parent.Kind() == SyntaxKind.CompilationUnit)
            {
                switch (source.Kind())
                {
                    case SyntaxKind.EnumDeclaration:
                    case SyntaxKind.NamespaceDeclaration:
                        return CodeItemAccessEnum.Public;
                    default:
                        return CodeItemAccessEnum.Internal;
                }
            }
            else
            {
                switch (source.Kind())
                {
                    case SyntaxKind.NamespaceDeclaration:
                    case SyntaxKind.EnumDeclaration:
                    case SyntaxKind.InterfaceDeclaration:
                        return CodeItemAccessEnum.Public;
                    default:
                        return CodeItemAccessEnum.Private;
                }
            }
        }

    }
}
