using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class BaseMapper
{
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
        codeItem.FullName = MapFullName(
            source,
            codeItemName,
            semanticModel);

        codeItem.FilePath =
            string.IsNullOrEmpty(source.SyntaxTree.FilePath)
                ? null
                : new Uri(source.SyntaxTree.FilePath);

        codeItem.Id = codeItem.FullName;
        codeItem.Tooltip = codeItemName;
        codeItem.Access = MapAccess(modifiers, source);
        codeItem.CodeDocumentViewModel = codeDocumentViewModel;

        codeItem.Span = source.Span;
        codeItem.IdentifierSpan = identifier?.Span;
        codeItem.OutlineSpan = MapOutlineSpan(
            codeItem.Span,
            codeItem.IdentifierSpan,
            nameSyntax?.Span);

        return codeItem;
    }

    /// <summary>
    /// Computes the outline span of a code item: the portion of its text that should be
    /// collapsible/foldable in the editor's outlining margin.
    /// </summary>
    /// <remarks>
    /// The outline span starts right after the item's name/identifier and runs to the end of
    /// the item's full <paramref name="span"/> — this excludes the signature itself (e.g. the
    /// <c>public void Foo(...)</c> part of a method) from the collapsible region, so folding
    /// only hides the body, not the declaration.
    /// <para>
    /// If both <paramref name="nameSpan"/> and <paramref name="identifierSpan"/> are supplied,
    /// <paramref name="identifierSpan"/> takes precedence as the start boundary. If neither is
    /// supplied, the outline span starts at position 0, effectively covering the entire
    /// <paramref name="span"/>.
    /// </para>
    /// </remarks>
    /// <param name="span">The full text span of the code item.</param>
    /// <param name="identifierSpan">
    /// The span of the item's identifier, if known. When present, its end position is used as
    /// the start of the outline span, taking priority over <paramref name="nameSpan"/>.
    /// </param>
    /// <param name="nameSpan">
    /// The span of the item's display name, if known. Used as the start of the outline span
    /// only when <paramref name="identifierSpan"/> is not supplied.
    /// </param>
    /// <returns>
    /// A <see cref="TextSpan"/> starting after the name/identifier and extending to the end of
    /// <paramref name="span"/>, used as the foldable region for this code item.
    /// </returns>
    private static TextSpan MapOutlineSpan(
        TextSpan span,
        TextSpan? identifierSpan,
        TextSpan? nameSpan)
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

        return new TextSpan(
            outlineSpanStart,
            span.End - outlineSpanStart);
    }

    private static string MapFullName(
        SyntaxNode source,
        string name,
        SemanticModel semanticModel)
    {
        try
        {
            var symbol = semanticModel.GetDeclaredSymbol(source);

            return symbol?.ToString() ?? name;
        }
        catch (Exception)
        {
            return name;
        }
    }

    private static string MapName(
        SyntaxToken? identifier,
        NameSyntax? nameSyntax,
        string name = "")
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

    private static CodeItemAccessEnum MapAccess(
        SyntaxTokenList? modifiers,
        SyntaxNode source)
    {
        if (modifiers == null)
        {
            return MapDefaultAccess(source);
        }

        if (modifiers.Value.Any(m =>
                m.IsKind(SyntaxKind.PublicKeyword)))
        {
            return CodeItemAccessEnum.Public;
        }

        if (modifiers.Value.Any(m =>
                m.IsKind(SyntaxKind.PrivateKeyword)))
        {
            return CodeItemAccessEnum.Private;
        }

        if (modifiers.Value.Any(m =>
                m.IsKind(SyntaxKind.ProtectedKeyword)))
        {
            return CodeItemAccessEnum.Protected;
        }

        if (modifiers.Value.Any(m =>
                m.IsKind(SyntaxKind.FriendKeyword)))
        {
            return CodeItemAccessEnum.Internal;
        }

        return MapDefaultAccess(source);
    }

    private static CodeItemAccessEnum MapDefaultAccess(
        SyntaxNode source)
    {
        if (source.Parent is CompilationUnitSyntax)
        {
            return source switch
            {
                EnumBlockSyntax => CodeItemAccessEnum.Public,
                NamespaceBlockSyntax => CodeItemAccessEnum.Public,
                _ => CodeItemAccessEnum.Internal,
            };
        }

        return source switch
        {
            NamespaceBlockSyntax => CodeItemAccessEnum.Public,
            EnumBlockSyntax => CodeItemAccessEnum.Public,
            InterfaceBlockSyntax => CodeItemAccessEnum.Public,
            _ => CodeItemAccessEnum.Private,
        };
    }
}