using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualStudio.Extensibility;
using System.Windows;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class MethodMapper
{
    public static CodeItem MapMethod(MethodBlockSyntax member, SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
        => MapMethod(member, member.SubOrFunctionStatement.Identifier, member.SubOrFunctionStatement.Modifiers,
            member.Body, member.ReturnType, member.SubOrFunctionStatement.ParameterList,
            CodeItemKindEnum.Method, semanticModel, codeDocumentViewModel);

    private static CodeItem MapMethod(SyntaxNode node, SyntaxToken identifier,
        SyntaxTokenList modifiers, BlockSyntax? body, TypeSyntax? returnType,
        ParameterListSyntax parameterList, CodeItemKindEnum kind,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        CodeItem codeItem;

        var statementsCodeItems = StatementMapper.MapStatement(body, semanticModel, codeDocumentViewModel);

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel, statementsCodeItems, codeDocumentViewModel.FilterRules);

        if (statementsCodeItems.Any(statement => statement.Visibility == Visibility.Visible))
        {
            // Map method as item containing statements
            codeItem = BaseMapper.MapBase<CodeClassItem>(node, semanticModel, codeDocumentViewModel, identifier, modifiers: modifiers);
            ((CodeClassItem)codeItem).Members.AddRange(statementsCodeItems);
        }
        else
        {
            // Map method as single item
            codeItem = BaseMapper.MapBase<CodeFunctionItem>(node, semanticModel, codeDocumentViewModel, identifier, modifiers: modifiers);

            var codeFunctionItem = codeItem as CodeFunctionItem;

            codeFunctionItem!.ReturnType = TypeMapper.Map(returnType);
            codeFunctionItem.Parameters = ParameterMapper.MapParameters(parameterList, semanticModel);
            codeItem.IdentifierSpan = identifier.Span;
            codeItem.Tooltip = TooltipMapper.Map(node, semanticModel, codeItem.Access, codeFunctionItem.ReturnType, codeItem.Name, parameterList);
        }

        codeItem.Id = IdMapper.MapId(codeItem.FullName, semanticModel, parameterList);
        codeItem.Kind = kind;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    public static CodeItem MapConstructor(ConstructorBlockSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(member, semanticModel, codeDocumentViewModel,
            member.SubNewStatement, modifiers: member.SubNewStatement.Modifiers);

        codeItem.Parameters = ParameterMapper.MapParameters(member.SubNewStatement.ParameterList, semanticModel);
        codeItem.Tooltip = TooltipMapper.Map(member, semanticModel, codeItem.Access, codeItem.ReturnType, codeItem.Name, member.SubNewStatement.ParameterList);
        codeItem.Id = IdMapper.MapId(member.SubNewStatement, semanticModel, member.SubNewStatement.ParameterList);
        codeItem.Kind = CodeItemKindEnum.Constructor;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.OverlayMoniker = ImageMoniker.KnownValues.Add;

        return codeItem;
    }
}
