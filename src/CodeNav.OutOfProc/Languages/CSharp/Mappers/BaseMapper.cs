using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public static class BaseMapper
{
    public static T MapBase<T>(
        SyntaxNode source,
        SyntaxToken identifier,
        SyntaxTokenList modifiers,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(
            source,
            identifier,
            modifiers,
            semanticModel,
            codeDocumentViewModel);

    public static T MapBase<T>(
        SyntaxNode source,
        NameSyntax name,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(
            source,
            identifier: null,
            name.ToString(),
            modifiers: [],
            semanticModel,
            codeDocumentViewModel);

    public static T MapBase<T>(
        SyntaxNode source,
        string name,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(
            source,
            identifier: null,
            name,
            modifiers: [],
            semanticModel,
            codeDocumentViewModel);

    public static T MapBase<T>(
        SyntaxNode source,
        SyntaxToken identifier,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
        => MapBase<T>(
            source,
            identifier,
            identifier.Text,
            modifiers: [],
            semanticModel,
            codeDocumentViewModel);

    /// <summary>
    /// Map commonly shared code item properties based on the syntaxt token that is been mapped
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Syntax node of the code member</param>
    /// <param name="identifier">Syntax token of the code identifier</param>
    /// <param name="name">Name of the code member</param>
    /// <param name="modifiers">Accessibility modifiers of the code member</param>
    /// <param name="semanticModel">Semantic model used during compilation</param>
    /// <param name="codeDocumentViewModel">Code document view model used in the CodeNav tool window</param>
    /// <returns>Code item class or othe code class derived from code item</returns>
    private static T MapBase<T>(
        SyntaxNode source,
        SyntaxToken? identifier,
        string name,
        SyntaxTokenList modifiers,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel) where T : CodeItem
    {
        var codeItem = Activator.CreateInstance<T>();

        var codeItemName = MapName(identifier, name);

        codeItem.Name = codeItemName;
        codeItem.FullName = MapFullName(source, codeItemName, semanticModel);
        codeItem.FilePath = string.IsNullOrEmpty(source.SyntaxTree.FilePath)
            ? null
            : new Uri(source.SyntaxTree.FilePath);
        codeItem.Id = codeItem.FullName;
        codeItem.Tooltip = codeItemName;
        codeItem.Access = MapAccess(modifiers, source);
        codeItem.CodeDocumentViewModel = codeDocumentViewModel;

        codeItem.Span = source.Span;
        codeItem.IdentifierSpan = identifier?.Span;
        codeItem.OutlineSpan = MapOutlineSpan(codeItem.Span, codeItem.IdentifierSpan, name); 

        return codeItem;
    }

    private static TextSpan MapOutlineSpan(TextSpan span, TextSpan? identifierSpan, string name)
    {
        var outlineSpanStart = span.Start;

        outlineSpanStart += identifierSpan != null
            ? identifierSpan.Value.Length
            : name.Length;

        return new TextSpan(outlineSpanStart, span.Length);
    }

    private static string MapFullName(SyntaxNode source, string name, SemanticModel semanticModel)
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

    private static string MapName(SyntaxToken? identifier, string name)
        => identifier != null ? identifier.Value.Text : name;

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
