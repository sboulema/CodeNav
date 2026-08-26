using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Windows;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class EnumMapper
{
    public static CodeItem MapEnum(
        EnumBlockSyntax member,
        SemanticModel semanticModel,
        SyntaxTree tree,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        CodeItem codeItem;

        var enumMembers = member
            .Members
            .OfType<EnumMemberDeclarationSyntax>()
            .Select(enumMember => MapEnumMember(enumMember, semanticModel, codeDocumentViewModel))
            .ToList();

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel, enumMembers, codeDocumentViewModel.FilterRules);

        if (enumMembers.Any(enumMember => enumMember.Visibility == Visibility.Visible))
        {
            // Map enum as item containing members
            codeItem = BaseMapper.MapBase<CodeClassItem>(
                member,
                semanticModel,
                codeDocumentViewModel,
                member.EnumStatement.Identifier,
                modifiers: member.EnumStatement.Modifiers);

            var regions = RegionMapper.MapRegions(
                tree,
                member.Span,
                codeDocumentViewModel);

            RegionMapper.AddRegionsIfNotPresent(
                ((CodeClassItem)codeItem).Members,
                regions);

            foreach (var enumMember in enumMembers)
            {
                if (RegionMapper.AddToRegion(regions, enumMember))
                {
                    continue;
                }

                ((CodeClassItem)codeItem).Members.Add(enumMember);
            }

            ((CodeClassItem)codeItem).Members.AddRange(enumMembers);
        }
        else
        {
            // Map enum as single item
            codeItem = BaseMapper.MapBase<CodeFunctionItem>(
                member,
                semanticModel,
                codeDocumentViewModel,
                member.EnumStatement.Identifier,
                modifiers: member.EnumStatement.Modifiers);
        }

        codeItem.Kind = CodeItemKindEnum.Enum;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

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