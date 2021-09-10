using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Windows.Media;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class NamespaceMapper
    {
        public static CodeNamespaceItem MapNamespace(NamespaceDeclarationSyntax member, 
            ICodeViewUserControl control, SemanticModel semanticModel, SyntaxTree tree)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeNamespaceItem>(member, member.Name, control, semanticModel);
            item.Kind = CodeItemKindEnum.Namespace;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderColor = Colors.DarkGray;
            item.IgnoreVisibility = VisibilityHelper.GetIgnoreVisibility(item);

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            var regions = RegionMapper.MapRegions(tree, member.Span, control);

            foreach (var namespaceMember in member.Members)
            {
                var memberItem = SyntaxMapper.MapMember(namespaceMember, tree, semanticModel, control);
                if (memberItem != null && !RegionMapper.AddToRegion(regions, memberItem))
                {
                    item.Members.Add(memberItem);
                }
            }

            // Add regions to namespace if they are not present in any children of the namespace
            if (regions.Any())
            {
                foreach (var region in regions)
                {
                    if (FindHelper.FindCodeItem(item.Members, region.Id) == null)
                    {
                        item.Members.Add(region);
                    }                   
                }
            }

            return item;
        }

        #if VS2022
        public static CodeNamespaceItem MapNamespace(BaseNamespaceDeclarationSyntax member,
            ICodeViewUserControl control, SemanticModel semanticModel, SyntaxTree tree)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeNamespaceItem>(member, member.Name, control, semanticModel);
            item.Kind = CodeItemKindEnum.Namespace;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderColor = Colors.DarkGray;
            item.IgnoreVisibility = VisibilityHelper.GetIgnoreVisibility(item);

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            var regions = RegionMapper.MapRegions(tree, member.Span, control);

            foreach (var namespaceMember in member.Members)
            {
                var memberItem = SyntaxMapper.MapMember(namespaceMember, tree, semanticModel, control);
                if (memberItem != null && !RegionMapper.AddToRegion(regions, memberItem))
                {
                    item.Members.Add(memberItem);
                }
            }

            // Add regions to namespace if they are not present in any children of the namespace
            if (regions.Any())
            {
                foreach (var region in regions)
                {
                    if (FindHelper.FindCodeItem(item.Members, region.Id) == null)
                    {
                        item.Members.Add(region);
                    }
                }
            }

            return item;
        }
        #endif

        public static CodeNamespaceItem MapNamespace(VisualBasicSyntax.NamespaceBlockSyntax member, 
            ICodeViewUserControl control, SemanticModel semanticModel, SyntaxTree tree)
        {
            if (member == null) return null;

            var item = BaseMapper.MapBase<CodeNamespaceItem>(member, member.NamespaceStatement.Name, control, semanticModel);
            item.Kind = CodeItemKindEnum.Namespace;
            item.Moniker = IconMapper.MapMoniker(item.Kind, item.Access);
            item.BorderColor = Colors.DarkGray;

            if (TriviaSummaryMapper.HasSummary(member) && SettingsHelper.UseXMLComments)
            {
                item.Tooltip = TriviaSummaryMapper.Map(member);
            }

            var regions = RegionMapper.MapRegions(tree, member.Span, control);

            foreach (var namespaceMember in member.Members)
            {
                var memberItem = SyntaxMapper.MapMember(namespaceMember, tree, semanticModel, control);
                if (memberItem != null && !RegionMapper.AddToRegion(regions, memberItem))
                {
                    item.Members.Add(memberItem);
                }
            }

            // Add regions to class if they have a region member inside them
            if (regions.Any())
            {
                foreach (var region in regions)
                {
                    if (FindHelper.FindCodeItem(item.Members, region.Id) == null)
                    {
                        item.Members.Add(region);
                    }
                }
            }

            return item;
        }
    }
}
