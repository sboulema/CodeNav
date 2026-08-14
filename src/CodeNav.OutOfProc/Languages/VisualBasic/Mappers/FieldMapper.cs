using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class FieldMapper
{
    public static IEnumerable<CodeItem> MapFields(
        FieldDeclarationSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
        => member.Declarators
            .SelectMany(declarator => declarator.Names)
            .Select(variable => MapField(
                member,
                variable,
                semanticModel,
                codeDocumentViewModel));

    private static CodeItem MapField(
        FieldDeclarationSyntax declaration,
        ModifiedIdentifierSyntax variable,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeItem>(
            variable,
            semanticModel,
            codeDocumentViewModel,
            variable.Identifier,
            modifiers: declaration.Modifiers);

        codeItem.Kind = IsConstant(declaration.Modifiers)
            ? CodeItemKindEnum.Constant
            : CodeItemKindEnum.Variable;

        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Tooltip = TooltipMapper.Map(
            variable,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            string.Empty);

        return codeItem;
    }

    private static bool IsConstant(
        SyntaxTokenList modifiers)
        => modifiers.Any(
            m => m.RawKind == (int)SyntaxKind.ConstKeyword);
}
