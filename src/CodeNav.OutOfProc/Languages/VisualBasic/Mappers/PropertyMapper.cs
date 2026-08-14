using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class PropertyMapper
{
    public static CodePropertyItem MapProperty(
        PropertyBlockSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var statement = member.PropertyStatement;

        var codeItem = BaseMapper.MapBase<CodePropertyItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            statement.Identifier,
            modifiers: statement.Modifiers);

        codeItem.IdentifierSpan = statement.Identifier.Span;

        codeItem.ReturnType = TypeMapper.Map(statement.AsClause);
        codeItem.Parameters = statement.ParameterList?.ToString() ?? string.Empty;

        var accessors = new List<string>();

        if (member.Accessors.Any(
                accessor => accessor.Kind() == SyntaxKind.GetAccessorBlock))
        {
            accessors.Add("Get");
        }

        if (member.Accessors.Any(
                accessor => accessor.Kind() == SyntaxKind.SetAccessorBlock))
        {
            accessors.Add("Set");
        }

        if (accessors.Any())
        {
            codeItem.Parameters +=
                $" {{{string.Join(",", accessors)}}}";
        }

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            codeItem.ReturnType,
            codeItem.Name,
            codeItem.Parameters);

        codeItem.Kind = CodeItemKindEnum.Property;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        return codeItem;
    }

    public static CodePropertyItem MapProperty(
        PropertyStatementSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodePropertyItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            member.Identifier,
            modifiers: member.Modifiers);

        codeItem.IdentifierSpan = member.Identifier.Span;
        codeItem.ReturnType = TypeMapper.Map(member.AsClause);
        codeItem.Parameters = member.ParameterList?.ToString() ?? string.Empty;
        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            codeItem.ReturnType,
            codeItem.Name,
            codeItem.Parameters);
        codeItem.Kind = CodeItemKindEnum.Property;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        return codeItem;
    }
}
