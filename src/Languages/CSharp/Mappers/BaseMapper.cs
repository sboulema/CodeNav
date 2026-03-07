using CodeNav.Constants;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public static class BaseMapper
{
    public static T MapBase<T>(SyntaxNode source, SyntaxToken identifier, SyntaxTokenList modifiers,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(source, identifier.Text, modifiers, semanticModel, codeDocumentViewModel);

    public static T MapBase<T>(SyntaxNode source, NameSyntax name,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(source, name.ToString(), [], semanticModel, codeDocumentViewModel);

    public static T MapBase<T>(SyntaxNode source, string name,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(source, name, [], semanticModel, codeDocumentViewModel);

    public static T MapBase<T>(SyntaxNode source, SyntaxToken identifier,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(source, identifier.Text, [], semanticModel, codeDocumentViewModel);

    private static T MapBase<T>(SyntaxNode source, string name, SyntaxTokenList modifiers,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
    {
        var element = Activator.CreateInstance<T>();

        element.Name = name;
        element.FullName = GetFullName(source, name, semanticModel);
        element.FilePath = string.IsNullOrEmpty(source.SyntaxTree.FilePath) ? null : new Uri(source.SyntaxTree.FilePath);
        element.Id = element.FullName;
        element.Tooltip = name;
        element.Span = source.Span;
        element.Access = MapAccess(modifiers, source);
        element.CodeDocumentViewModel = codeDocumentViewModel;

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

    private static CodeItemAccessEnum MapAccess(SyntaxTokenList modifiers, SyntaxNode source)
    {
        if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.SealedKeyword))
        {
            return CodeItemAccessEnum.Sealed;
        }
        if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.PublicKeyword))
        {
            return CodeItemAccessEnum.Public;
        }
        if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.PrivateKeyword))
        {
            return CodeItemAccessEnum.Private;
        }
        if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.ProtectedKeyword))
        {
            return CodeItemAccessEnum.Protected;
        }
        if (modifiers.Any(m => m.RawKind == (int)SyntaxKind.InternalKeyword))
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
        if (source.Parent.IsKind(SyntaxKind.CompilationUnit))
        {
            return source.Kind() switch
            {
                SyntaxKind.EnumDeclaration => CodeItemAccessEnum.Public,
                SyntaxKind.NamespaceDeclaration => CodeItemAccessEnum.Public,
                _ => CodeItemAccessEnum.Internal,
            };
        }

        return source.Kind() switch
        {
            SyntaxKind.NamespaceDeclaration => CodeItemAccessEnum.Public,
            SyntaxKind.EnumDeclaration => CodeItemAccessEnum.Public,
            SyntaxKind.InterfaceDeclaration => CodeItemAccessEnum.Public,
            _ => CodeItemAccessEnum.Private,
        };
    }
}
