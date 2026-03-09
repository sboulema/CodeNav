using CodeNav.Constants;
using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Extensibility;
using System.Windows;

namespace CodeNav.Languages.CSharp.Mappers;

public static class MethodMapper
{
    public static CodeItem MapMethod(MethodDeclarationSyntax member, SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
        => MapMethod(member, member.Identifier, member.Modifiers,
            member.Body, member.ReturnType, member.ParameterList,
            CodeItemKindEnum.Method, semanticModel, codeDocumentViewModel);

    public static CodeItem MapMethod(LocalFunctionStatementSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
        => MapMethod(member, member.Identifier, member.Modifiers,
            member.Body, member.ReturnType, member.ParameterList,
            CodeItemKindEnum.LocalFunction, semanticModel, codeDocumentViewModel);

    private static CodeItem MapMethod(SyntaxNode node, SyntaxToken identifier,
        SyntaxTokenList modifiers, BlockSyntax? body, TypeSyntax? returnType,
        ParameterListSyntax parameterList, CodeItemKindEnum kind,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        CodeItem codeItem;

        var statementsCodeItems = StatementMapper.MapStatement(body, semanticModel, codeDocumentViewModel);

        VisibilityHelper.SetCodeItemVisibility(statementsCodeItems, codeDocumentViewModel.FilterRules);

        if (statementsCodeItems.Any(statement => statement.Visibility == Visibility.Visible))
        {
            // Map method as item containing statements
            codeItem = BaseMapper.MapBase<CodeClassItem>(node, identifier,modifiers, semanticModel, codeDocumentViewModel);
            ((CodeClassItem)codeItem).Members.AddRange(statementsCodeItems);
        }
        else
        {
            // Map method as single item
            codeItem = BaseMapper.MapBase<CodeFunctionItem>(node, identifier, modifiers, semanticModel, codeDocumentViewModel);

            var codeFunctionItem = codeItem as CodeFunctionItem;

            codeFunctionItem!.ReturnType = TypeMapper.Map(returnType);
            codeFunctionItem.Parameters = ParameterMapper.MapParameters(parameterList);
            codeItem.IdentifierSpan = identifier.Span;
            codeItem.Tooltip = TooltipMapper.Map(node, codeItem.Access, codeFunctionItem.ReturnType, codeItem.Name, parameterList);
        }

        codeItem.Id = IdMapper.MapId(codeItem.FullName, parameterList);
        codeItem.Kind = kind;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    public static CodeItem MapConstructor(ConstructorDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(member, member.Identifier, member.Modifiers, semanticModel, codeDocumentViewModel);

        codeItem.Parameters = ParameterMapper.MapParameters(member.ParameterList);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, codeItem.ReturnType, codeItem.Name, member.ParameterList);
        codeItem.Id = IdMapper.MapId(member.Identifier, member.ParameterList);
        codeItem.Kind = CodeItemKindEnum.Constructor;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.OverlayMoniker = ImageMoniker.KnownValues.Add;

        return codeItem;
    }
}
