using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Extensibility;
using System.Windows;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

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
            codeFunctionItem.Parameters = ParameterMapper.MapParameters(parameterList);
            codeItem.IdentifierSpan = identifier.Span;
            codeItem.Tooltip = TooltipMapper.Map(node, codeItem.Access, codeFunctionItem.ReturnType, codeItem.Name, parameterList);
        }

        codeItem.Id = IdMapper.MapId(codeItem.FullName, parameterList);
        codeItem.Kind = kind;
        codeItem.Moniker = IsExtensionMethod(node, semanticModel)
            ? ImageMoniker.KnownValues.ExtensionMethod
            : IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    public static CodeItem MapConstructor(ConstructorDeclarationSyntax member,
        SemanticModel semanticModel, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeFunctionItem>(member, semanticModel, codeDocumentViewModel, member.Identifier, modifiers: member.Modifiers);

        codeItem.Parameters = ParameterMapper.MapParameters(member.ParameterList);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, codeItem.ReturnType, codeItem.Name, member.ParameterList);
        codeItem.Id = IdMapper.MapId(member.Identifier, member.ParameterList);
        codeItem.Kind = CodeItemKindEnum.Constructor;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    /// <summary>
    /// Determines whether the given syntax node declares an extension method.
    /// </summary>
    /// <param name="node">
    /// The syntax node to check, typically a <see cref="MethodDeclarationSyntax"/>.
    /// </param>
    /// <param name="semanticModel">
    /// The semantic model used to resolve the declared symbol. It must belong to the same
    /// syntax tree as <paramref name="node"/>, which is not the case for members that are mapped
    /// from a base class declared in another file. Use <c>ForNode</c> to obtain a matching model.
    /// </param>
    /// <returns>
    /// <c>true</c> if the node declares a method whose symbol is an extension method;<br/>
    /// <c>false</c> if it does not.
    /// </returns>
    private static bool IsExtensionMethod(SyntaxNode node, SemanticModel semanticModel)
        => semanticModel.GetDeclaredSymbol(node) is IMethodSymbol { IsExtensionMethod: true };
}
