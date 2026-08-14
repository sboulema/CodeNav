using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class NamespaceMapper
{
    public static CodeNamespaceItem MapNamespace(
        NamespaceBlockSyntax member,
        SemanticModel semanticModel,
        SyntaxTree tree,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeNamespaceItem>(
            member,
            semanticModel,
            codeDocumentViewModel,
            name: member.NamespaceStatement.Name.ToString());

        codeItem.Kind = CodeItemKindEnum.Namespace;
        codeItem.Moniker = IconMapper.MapMoniker(
            codeItem.Kind,
            codeItem.Access);

        codeItem.Tooltip = TooltipMapper.Map(
            member,
            codeItem.Access,
            string.Empty,
            codeItem.Name,
            string.Empty);

        var regions = RegionMapper.MapRegions(
            tree,
            member.Span,
            codeDocumentViewModel);

        foreach (var namespaceMember in member.Members)
        {
            foreach (var memberItem in DocumentMapper.MapMembers(
                namespaceMember,
                tree,
                semanticModel,
                codeDocumentViewModel))
            {
                if (!RegionMapper.AddToRegion(regions, memberItem))
                {
                    codeItem.Members.AddIfNotNull(memberItem);
                }
            }
        }

        RegionMapper.AddRegionsIfNotPresent(
            codeItem.Members,
            regions);

        return codeItem;
    }
}
