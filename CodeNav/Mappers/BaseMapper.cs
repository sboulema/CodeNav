using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Windows.Media;

namespace CodeNav.Mappers
{
    public static class BaseMapper
    {
        public static T MapBase<T>(SyntaxNode source, SyntaxToken identifier, SyntaxTokenList modifiers, CodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, identifier.Text, modifiers, control, semanticModel);
        }

        public static T MapBase<T>(SyntaxNode source, NameSyntax name, CodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, name.ToString(), new SyntaxTokenList(), control, semanticModel);
        }

        public static T MapBase<T>(SyntaxNode source, string name, CodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, name, new SyntaxTokenList(), control, semanticModel);
        }

        public static T MapBase<T>(SyntaxNode source, SyntaxToken identifier, CodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            return MapBase<T>(source, identifier.Text, new SyntaxTokenList(), control, semanticModel);
        }

        private static T MapBase<T>(SyntaxNode source, string name, SyntaxTokenList modifiers, CodeViewUserControl control, SemanticModel semanticModel) where T : CodeItem
        {
            var element = Activator.CreateInstance<T>();

            element.Name = name;
            element.FullName = GetFullName(source, name, semanticModel);
            element.Id = element.FullName;
            element.Tooltip = name;
            element.StartLine = GetStartLine(source);
            element.StartLinePosition = GetStartLinePosition(source);
            element.EndLine = GetEndLine(source);
            element.Foreground = ColorHelper.CreateSolidColorBrush(Colors.Black);
            element.Access = MapAccess(modifiers, source);
            element.FontSize = Settings.Default.Font.SizeInPoints;
            element.ParameterFontSize = Settings.Default.Font.SizeInPoints - 1;
            element.FontFamily = new FontFamily(Settings.Default.Font.FontFamily.Name);
            element.FontStyle = FontStyleMapper.Map(Settings.Default.Font.Style);
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

        private static int GetStartLine(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).StartLinePosition.Line + 1;

        private static int GetEndLine(SyntaxNode source) =>
            source.SyntaxTree.GetLineSpan(source.Span).EndLinePosition.Line + 1;

        private static CodeItemAccessEnum MapAccess(SyntaxTokenList modifiers, SyntaxNode source)
        {
            if (modifiers.Any(m => m.Kind() == SyntaxKind.SealedKeyword))
            {
                return CodeItemAccessEnum.Sealed;
            }
            if (modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword))
            {
                return CodeItemAccessEnum.Public;
            }
            if (modifiers.Any(m => m.Kind() == SyntaxKind.PrivateKeyword))
            {
                return CodeItemAccessEnum.Private;
            }
            if (modifiers.Any(m => m.Kind() == SyntaxKind.ProtectedKeyword))
            {
                return CodeItemAccessEnum.Protected;
            }
            if (modifiers.Any(m => m.Kind() == SyntaxKind.InternalKeyword))
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
