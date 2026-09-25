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
    /// Creates and populates a new <typeparamref name="T"/> instance with common code-item metadata
    /// derived from a syntax node, such as name, full name, file path, access modifier, and span information.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="CodeItem"/> to create.</typeparam>
    /// <param name="source">The syntax node the code item is derived from.</param>
    /// <param name="semanticModel">The semantic model used to resolve the item's full name.</param>
    /// <param name="codeDocumentViewModel">The view model of the document the code item belongs to.</param>
    /// <param name="identifier">The identifier token used to derive the name and identifier span, if available.</param>
    /// <param name="nameSyntax">The name syntax used to derive the name and outline span, if available.</param>
    /// <param name="name">A fallback name to use when neither <paramref name="identifier"/> nor <paramref name="nameSyntax"/> is provided.</param>
    /// <param name="modifiers">The syntax token list used to determine the item's access modifier.</param>
    /// <returns>A new <typeparamref name="T"/> instance populated with metadata from <paramref name="source"/>.</returns>

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
        codeItem.SpanStartLinePosition = MapStartLinePosition(source.SyntaxTree, source.Span)!.Value;
        codeItem.SpanEndLinePosition = MapEndLinePosition(source.SyntaxTree, source.Span)!.Value;

        codeItem.IdentifierSpan = identifier?.Span;
        codeItem.IdentifierSpanStartLinePosition = MapStartLinePosition(source.SyntaxTree, identifier?.Span);
        codeItem.IdentifierSpanEndLinePosition = MapEndLinePosition(source.SyntaxTree, source.Span)!.Value;

        codeItem.OutlineSpan = MapOutlineSpan(codeItem.Span, codeItem.IdentifierSpan, nameSyntax?.Span);
        codeItem.OutlineSpanStartLinePosition = MapStartLinePosition(source.SyntaxTree, codeItem.OutlineSpan);

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
        if (identifier != null &&
            !identifier.Value.IsKind(SyntaxKind.None))
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

    /// <summary>
    /// Gets the zero-based line and column position for the start of the specified span.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree the span belongs to.</param>
    /// <param name="span">The span to resolve a line position for, or <see langword="null"/>.</param>
    /// <returns>
    /// The zero-based <see cref="LinePosition"/> for the start of <paramref name="span"/>,
    /// or <see langword="null"/> if <paramref name="span"/> is <see langword="null"/>.
    /// </returns>
    public static LinePosition? MapStartLinePosition(SyntaxTree syntaxTree, TextSpan? span)
        => span == null
            ? null
            : syntaxTree.GetLineSpan(span.Value).StartLinePosition;

    /// <summary>
    /// Gets the zero-based line and column position for the end of the specified span.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree the span belongs to.</param>
    /// <param name="span">The span to resolve a line position for, or <see langword="null"/>.</param>
    /// <returns>
    /// The zero-based <see cref="LinePosition"/> for the end of <paramref name="span"/>,
    /// or <see langword="null"/> if <paramref name="span"/> is <see langword="null"/>.
    /// </returns>
    public static LinePosition? MapEndLinePosition(SyntaxTree syntaxTree, TextSpan? span)
        => span == null
            ? null
            : syntaxTree.GetLineSpan(span.Value).EndLinePosition;
}
