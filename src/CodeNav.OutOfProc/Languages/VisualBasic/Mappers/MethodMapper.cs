using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class MethodMapper
{
    public static CodeItem MapMethod(
        MethodBlockSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var statement = member.SubOrFunctionStatement;

        var identifier = statement.Identifier;
        var modifiers = statement.Modifiers;

        var returnType = statement.AsClause;
        var parameters = statement.ParameterList;

        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            identifier,
            modifiers: modifiers);

        var functionItem = codeItem;

        functionItem.ReturnType = TypeMapper.Map(returnType);
        functionItem.Parameters = parameters?.ToString() ?? string.Empty;

        codeItem.IdentifierSpan = identifier.Span;
        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            functionItem.ReturnType,
            codeItem.Name,
            functionItem.Parameters);

        codeItem.Kind = CodeItemKindEnum.Method;

        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        return codeItem;
    }

    public static CodeItem MapMethod(
        MethodStatementSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            member.Identifier,
            modifiers: member.Modifiers);

        codeItem.ReturnType = TypeMapper.Map(member.AsClause);
        codeItem.Parameters = member.ParameterList?.ToString() ?? string.Empty;
        codeItem.IdentifierSpan = member.Identifier.Span;
        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            codeItem.ReturnType,
            codeItem.Name,
            codeItem.Parameters);
        codeItem.Kind = CodeItemKindEnum.Method;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        return codeItem;
    }

    public static CodeItem MapConstructor(
        ConstructorBlockSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var statement = member.SubNewStatement;

        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            name: "New",
            modifiers: statement.Modifiers);

        var functionItem = codeItem;

        functionItem.Parameters =
            statement.ParameterList?.ToString() ?? string.Empty;

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            functionItem.Parameters);

        codeItem.Kind = CodeItemKindEnum.Constructor;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        return codeItem;
    }
}
