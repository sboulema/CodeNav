using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Languages.TypeScript.Parsing;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.OutOfProc.Languages.TypeScript.Mappers;

public static class BaseMapper
{
    /// <summary>
    /// Map commonly shared code item properties based on the parsed TypeScript node.
    /// </summary>
    /// <typeparam name="T">CodeItem or a type derived from it</typeparam>
    /// <param name="node">Parsed TypeScript node</param>
    /// <param name="codeDocumentViewModel">Code document view model used in the CodeNav tool window</param>
    /// <param name="parentFullName">Dotted full name of the containing namespace/class/interface, used to build a unique id</param>
    /// <param name="isMember">Whether this node is a class/interface member (used to compute the default access modifier)</param>
    public static T MapBase<T>(
        TypeScriptNode node,
        CodeDocumentViewModel codeDocumentViewModel,
        SourceText sourceText,
        string parentFullName,
        bool isMember) where T : CodeItem, new()
    {
        var codeItem = new T();

        var fullName = string.IsNullOrEmpty(parentFullName)
            ? node.Name
            : $"{parentFullName}.{node.Name}";

        codeItem.Name = node.Name;
        codeItem.FullName = fullName;
        // FilePath is intentionally left null: this mapper only ever produces items that live in
        // the currently open document, so there is never a need to open a different file on click.
        codeItem.Id = fullName + node.Parameters;
        codeItem.Tooltip = node.Name;
        codeItem.Access = MapAccess(node, isMember);
        codeItem.CodeDocumentViewModel = codeDocumentViewModel;

        codeItem.Span = node.Span;
        codeItem.SpanStartLinePosition = SpanHelper.MapStartLinePosition(sourceText, node.Span)!.Value;
        codeItem.SpanEndLinePosition = SpanHelper.MapEndLinePosition(sourceText, node.Span)!.Value;

        codeItem.IdentifierSpan = node.IdentifierSpan;
        codeItem.IdentifierSpanStartLinePosition = SpanHelper.MapStartLinePosition(sourceText, node.IdentifierSpan);
        codeItem.IdentifierSpanEndLinePosition = SpanHelper.MapEndLinePosition(sourceText, node.IdentifierSpan);

        codeItem.OutlineSpan = MapOutlineSpan(node.Span, node.IdentifierSpan);
        codeItem.OutlineSpanStartLinePosition = SpanHelper.MapStartLinePosition(sourceText, codeItem.OutlineSpan);

        return codeItem;
    }

    private static TextSpan MapOutlineSpan(TextSpan span, TextSpan identifierSpan)
    {
        var outlineStart = identifierSpan.End > 0 ? identifierSpan.End : span.Start;
        return new TextSpan(outlineStart, Math.Max(0, span.End - outlineStart));
    }

    /// <summary>
    /// Map the access modifier of a TypeScript declaration.
    /// </summary>
    /// <remarks>
    /// TypeScript class/interface members default to public when no modifier is present. Top-level
    /// (or namespace-level) declarations have no real access modifier of their own; whether they're
    /// visible outside the module depends only on the "export" keyword, which is mapped to
    /// Public/Internal here to stay consistent with the C#/VB mappers' default-access convention.
    /// </remarks>
    private static CodeItemAccessEnum MapAccess(TypeScriptNode node, bool isMember)
    {
        if (node.HasModifier("private"))
        {
            return CodeItemAccessEnum.Private;
        }

        if (node.HasModifier("protected"))
        {
            return CodeItemAccessEnum.Protected;
        }

        if (node.HasModifier("public"))
        {
            return CodeItemAccessEnum.Public;
        }

        if (isMember)
        {
            return CodeItemAccessEnum.Public;
        }

        return node.HasModifier("export")
            ? CodeItemAccessEnum.Public
            : CodeItemAccessEnum.Internal;
    }
}
