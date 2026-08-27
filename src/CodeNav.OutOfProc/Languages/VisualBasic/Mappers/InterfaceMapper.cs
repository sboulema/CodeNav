using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Windows;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class InterfaceMapper
{
    public static CodeItem MapInterface(
        InterfaceBlockSyntax member,
        SemanticModel semanticModel,
        SyntaxTree tree,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        CodeItem codeItem;

        var interfaceMembers = member
            .Members
            .Select(interfaceMember => DocumentMapper.MapMember(interfaceMember, tree, semanticModel, codeDocumentViewModel))
            .FilterNull()
            .ToList();

        interfaceMembers
            .ForEach(interfaceMember => interfaceMember.AdditionalKinds.Add(CodeItemKindEnum.InterfaceMember));

        VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel, interfaceMembers, codeDocumentViewModel.FilterRules);

        if (interfaceMembers.Any(interfaceMember => interfaceMember.Visibility == Visibility.Visible))
        {
            codeItem = BaseMapper.MapBase<CodeInterfaceItem>(
                member,
                semanticModel,
                codeDocumentViewModel,
                member.InterfaceStatement.Identifier,
                modifiers: member.InterfaceStatement.Modifiers);

            ((CodeInterfaceItem)codeItem).Parameters = MapInheritance(member);
            codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, ((CodeInterfaceItem)codeItem).Parameters);

            var regions = RegionMapper.MapRegions(tree, member.Span, codeDocumentViewModel);

            foreach (var interfaceMember in member.Members)
            {
                foreach (var memberItem in DocumentMapper.MapMembers(
                    interfaceMember,
                    tree,
                    semanticModel,
                    codeDocumentViewModel))
                {
                    if (memberItem == null)
                    {
                        continue;
                    }

                    memberItem.AdditionalKinds.Add(CodeItemKindEnum.InterfaceMember);

                    if (RegionMapper.AddToRegion(regions, memberItem))
                    {
                        continue;
                    }

                    ((CodeInterfaceItem)codeItem).Members.Add(memberItem);
                }
            }

            RegionMapper.AddRegionsIfNotPresent(((CodeInterfaceItem)codeItem).Members, regions);
        }
        else
        {
            codeItem = BaseMapper.MapBase<CodePropertyItem>(
                member,
                semanticModel,
                codeDocumentViewModel,
                member.InterfaceStatement.Identifier,
                modifiers: member.InterfaceStatement.Modifiers);

            ((CodePropertyItem)codeItem).Parameters = MapInheritance(member);
            codeItem.Tooltip = TooltipMapper.Map(member, codeItem.Access, string.Empty, codeItem.Name, ((CodePropertyItem)codeItem).Parameters);
        }

        codeItem.Kind = CodeItemKindEnum.Interface;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        return codeItem;
    }

    private static string MapInheritance(
        InterfaceBlockSyntax member)
    {
        var inheritance = new List<string>();

        foreach (var inheritsStatement in member.Inherits)
        {
            inheritance.AddRange(
                inheritsStatement.Types.Select(
                    type => type.ToString()));
        }

        return inheritance.Any()
            ? $" : {string.Join(", ", inheritance)}"
            : string.Empty;
    }
}
