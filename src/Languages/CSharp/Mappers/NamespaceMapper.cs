using CodeNav.Constants;
using CodeNav.Extensions;
using CodeNav.Mappers;
using CodeNav.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeNav.Languages.CSharp.Mappers;

public static class NamespaceMapper
{
    public static CodeNamespaceItem MapNamespace(BaseNamespaceDeclarationSyntax member,
        SemanticModel semanticModel, SyntaxTree tree, CodeDocumentViewModel codeDocumentViewModel)
    {
        var codeItem = BaseMapper.MapBase<CodeNamespaceItem>(member, member.Name, semanticModel, codeDocumentViewModel);
        codeItem.Kind = CodeItemKindEnum.Namespace;
        codeItem.Moniker = IconMapper.MapMoniker(codeItem.Kind, codeItem.Access);

        if (TriviaSummaryMapper.HasSummary(member))
        {
            codeItem.Tooltip = TriviaSummaryMapper.Map(member);
        }

        var regions = RegionMapper.MapRegions(tree, member.Span, codeDocumentViewModel);

        foreach (var namespaceMember in member.Members)
        {
            var memberItem = DocumentMapper.MapMember(namespaceMember, tree, semanticModel, codeDocumentViewModel);
            if (memberItem != null && !RegionMapper.AddToRegion(regions, memberItem))
            {
                codeItem.Members.AddIfNotNull(memberItem);
            }
        }

        // Add regions to namespace if they are not present in any children of the namespace
        if (regions.Any())
        {
            foreach (var region in regions)
            {
                if (codeItem.Members.Flatten().FilterNull().Any(i => i.Id == region?.Id) == false)
                {
                    codeItem.Members.AddIfNotNull(region);
                }
            }
        }

        return codeItem;
    }
}
