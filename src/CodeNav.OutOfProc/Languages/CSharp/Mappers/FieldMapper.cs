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
        var item = BaseMapper.MapBase<CodeItem>(member, semanticModel, codeDocumentViewModel, member.Declaration.Variables.First().Identifier,
            modifiers: member.Modifiers);

        item.Kind = IsConstant(member.Modifiers)
            ? CodeItemKindEnum.Constant
            : CodeItemKindEnum.Variable;
        item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);

        if (TriviaSummaryMapper.HasSummary(member))
        {
            item.Tooltip = TriviaSummaryMapper.Map(member);
        }

        return item;
    }

    private static bool IsConstant(SyntaxTokenList modifiers)
        => modifiers.Any(m => m.RawKind == (int)SyntaxKind.ConstKeyword);
}
