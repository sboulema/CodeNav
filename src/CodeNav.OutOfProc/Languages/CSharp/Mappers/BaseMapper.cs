using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public static class BaseMapper
{
    /// <summary>
    /// Map commonly shared code item properties based on the syntaxt token that is been mapped
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Syntax node of the code member</param>
    /// <param name="semanticModel">Semantic model used during compilation</param>
    /// <param name="codeDocumentViewModel">Code document view model used in the CodeNav tool window</param>
    /// <param name="identifier">Syntax token of the code member identifier</param>
    /// <param name="nameSyntax">Syntax token of the code member name</param>
    /// <param name="name">Name of the code member</param>
    /// <param name="modifiers">Accessibility modifiers of the code member</param>
    /// <returns>Code item class or othe code class derived from code item</returns>
    public static T MapBase<T>(
        SyntaxNode source,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel,
        SyntaxToken? identifier = null,
        NameSyntax? nameSyntax = null,
        string name = "",
        SyntaxTokenList? modifiers = null) where T : CodeItem
    {
        var codeItem = Activator.CreateInstance<T>();

        var codeItemName = MapName(identifier, nameSyntax, name);

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
        codeItem.OutlineSpan = MapOutlineSpan(codeItem.Span, codeItem.IdentifierSpan, nameSyntax?.Span); 

        return codeItem;
    }

    /// <summary>
    /// Map the span that is used for expanding/collapsing outline regions
    /// </summary>
    /// <param name="span">Normal span of the syntax node</param>
    /// <param name="identifierSpan">Identifier span of the syntax node</param>
    /// <param name="name">Name of the syntax node</param>
    /// <returns>TextSpan usable for outlining</returns>
    private static TextSpan MapOutlineSpan(TextSpan span, TextSpan? identifierSpan, TextSpan? nameSpan)
    {
        var outlineSpanStart = 0;

        if (nameSpan != null)
        {
            outlineSpanStart = nameSpan.Value.End;
        }

        if (identifierSpan != null)
        {
            outlineSpanStart = identifierSpan.Value.End;
        }

        return new TextSpan(outlineSpanStart, span.End - outlineSpanStart);
    }

    /// <summary>
    /// Map the full name of a code item
    /// </summary>
    /// <remarks>Used to create a unique id for the code item</remarks>
    /// <param name="source">Syntax node of the code item</param>
    /// <param name="name">Display name of the code item</param>
    /// <param name="semanticModel">Semantic model used during compilation</param>
    /// <returns>String full name</returns>
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

    /// <summary>
    /// Map the display name of a code item
    /// </summary>
    /// <param name="identifier">Identifier syntax token of the code item</param>
    /// <param name="nameSyntax">Name syntax token of the code item</param>
    /// <returns>String display name</returns>
    private static string MapName(SyntaxToken? identifier, NameSyntax? nameSyntax, string name = "")
    {
        if (identifier != null)
        {
            return identifier.Value.Text;
        }

        if (nameSyntax != null)
        {
            return nameSyntax.ToString();
        }

        return name;
    }

    private static CodeItemAccessEnum MapAccess(SyntaxTokenList? modifiers, SyntaxNode source)
    {
        if (modifiers == null)
        {
            return MapDefaultAccess(source);
        }

        if (modifiers.Value.Any(m => m.RawKind == (int)SyntaxKind.SealedKeyword))
        {
            return CodeItemAccessEnum.Sealed;
        }
        if (modifiers.Value.Any(m => m.RawKind == (int)SyntaxKind.PublicKeyword))
        {
            return CodeItemAccessEnum.Public;
        }
        if (modifiers.Value.Any(m => m.RawKind == (int)SyntaxKind.PrivateKeyword))
        {
            return CodeItemAccessEnum.Private;
        }
        if (modifiers.Value.Any(m => m.RawKind == (int)SyntaxKind.ProtectedKeyword))
        {
            return CodeItemAccessEnum.Protected;
        }
        if (modifiers.Value.Any(m => m.RawKind == (int)SyntaxKind.InternalKeyword))
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
