using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class EventMapper
{
    public static CodeItem MapEvent(
        EventBlockSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var statement = member.EventStatement;

        var codeItem = BaseMapper.MapBase<CodeItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            statement.Identifier,
            modifiers: statement.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Event;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            string.Empty);

        return codeItem;
    }

    public static CodeItem MapEvent(
        EventStatementSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            member.Identifier,
            modifiers: member.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Event;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);
        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            string.Empty);

        return codeItem;
    }
}
