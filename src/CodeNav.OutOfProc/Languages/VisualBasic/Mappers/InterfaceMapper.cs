using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class InterfaceMapper
{
    public static CodeInterfaceItem MapInterface(
        InterfaceBlockSyntax member,
        SemanticModel semanticModel,
        SyntaxTree tree,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeInterfaceItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            member.InterfaceStatement.Identifier,
            modifiers: member.InterfaceStatement.Modifiers);

        codeItem.Kind = CodeItemKindEnum.Interface;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Parameters = MapInheritance(member);

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            codeItem.Parameters);

        var regions = RegionMapper.MapRegions(
            tree,
            member.Span,
            codeDocumentViewModel);

        foreach (var interfaceMember in member.Members)
        {
            foreach (var memberItem in DocumentMapper.MapMembers(
                interfaceMember,
                tree,
                semanticModel,
                codeDocumentViewModel))
            {
                if (RegionMapper.AddToRegion(regions, memberItem))
                {
                    continue;
                }

                codeItem.Members.Add(memberItem);
            }
        }

        RegionMapper.AddRegionsIfNotPresent(
            codeItem.Members,
            regions);

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
