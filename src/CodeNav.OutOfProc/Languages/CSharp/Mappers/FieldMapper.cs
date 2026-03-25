using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public static class FieldMapper
{
    public static CodeItem MapField(FieldDeclarationSyntax member, SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeItem>(member, semanticModel, codeDocumentViewModel, member.Declaration.Variables.First().Identifier,
            modifiers: member.Modifiers);

        codeItem.Kind = IsConstant(member.Modifiers)
            ? CodeItemKindEnum.Constant
            : CodeItemKindEnum.Variable;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);
        codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, string.Empty);

        return codeItem;
    }

    private static bool IsConstant(SyntaxTokenList modifiers)
        => modifiers.Any(m => m.RawKind == (int)SyntaxKind.ConstKeyword);
}
