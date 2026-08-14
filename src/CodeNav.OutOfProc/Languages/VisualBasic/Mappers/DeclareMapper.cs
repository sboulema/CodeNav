using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class DeclareMapper
{
    public static CodeItem MapDeclare(
        DeclareStatementSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            member.Identifier,
            modifiers: member.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Method;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Parameters =
            member.ParameterList?.ToString() ?? string.Empty;

        codeItem.ReturnType = TypeMapper.Map(member.AsClause);

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            codeItem.ReturnType,
            codeItem.Name,
            codeItem.Parameters);

        return codeItem;
    }
}