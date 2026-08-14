using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class OperatorMapper
{
    public static CodeItem MapOperator(
        OperatorStatementSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
        => MapOperator(
            member,
            member,
            semanticModel,
            codeDocumentViewModel);

    public static CodeItem MapOperator(
        OperatorBlockSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
        => MapOperator(
            member,
            member.OperatorStatement,
            semanticModel,
            codeDocumentViewModel);

    private static CodeItem MapOperator(
        SyntaxNode source,
        OperatorStatementSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(
            source,
            semanticModel,
            codeDocumentViewModel,
            name: member.OperatorToken.Text,
            modifiers: member.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Method;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Parameters =
            member.ParameterList?.ToString() ?? string.Empty;

        codeItem.ReturnType = TypeMapper.Map(member.AsClause);

        codeItem.Tooltip = TooltipMapper.Map(
            source,
            codeItem.Access,
            codeItem.ReturnType,
            codeItem.Name,
            codeItem.Parameters);

        return codeItem;
    }
}
