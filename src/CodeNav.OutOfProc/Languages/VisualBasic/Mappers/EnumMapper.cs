using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class EnumMapper
{
    public static CodeItem MapEnum(
        EnumBlockSyntax member,
        SemanticModel semanticModel,
        SyntaxTree tree,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var statement = member.EnumStatement;

        var codeItem = BaseMapper.MapBase<CodeClassItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            statement.Identifier,
            modifiers: statement.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Enum;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        var regions = RegionMapper.MapRegions(
            tree,
            member.Span,
            codeDocumentViewModel);

        foreach (var enumMember in member.Members.OfType<EnumMemberDeclarationSyntax>())
        {
            var memberItem = MapEnumMember(
                enumMember,
                semanticModel,
                codeDocumentViewModel);

            if (RegionMapper.AddToRegion(regions, memberItem))
            {
                continue;
            }

            codeItem.Members.Add(memberItem);
        }

        RegionMapper.AddRegionsIfNotPresent(
            codeItem.Members,
            regions);

        return codeItem;
    }

    private static CodeItem MapEnumMember(
        EnumMemberDeclarationSyntax member,
        SemanticModel semanticModel,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            member.Identifier);

        codeItem.Kind = CodeItemKindEnum.EnumMember;
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