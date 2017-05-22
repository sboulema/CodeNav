using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CodeNav.Mappers
{
    public static class RegionMapper
    {
        public static List<CodeRegionItem> MapRegions(SyntaxTree tree, TextSpan span)
        {
            var root = tree.GetRoot();
            var regionList = new List<CodeRegionItem>();

            foreach (var regionDirective in root.DescendantTrivia().Where(i => i.Kind() == SyntaxKind.RegionDirectiveTrivia && span.Contains(i.Span)))
            {
                regionList.Add(MapRegion(regionDirective));
            }
               
            foreach (var endRegionDirective in root.DescendantTrivia().Where(j => j.Kind() == SyntaxKind.EndRegionDirectiveTrivia && span.Contains(j.Span)))
            {
                var reg = regionList.Last(x => x.StartLine < GetStartLine(endRegionDirective) && x.EndLine == 0);
                reg.EndLine = GetEndLine(endRegionDirective);
            }

            var list = ToHierarchy(regionList, int.MinValue, int.MaxValue);

            return list.Select(r => r as CodeRegionItem).ToList();
        }

        /// <summary>
        /// Transform the flat list of Regions to a nested List based on the start and end lines of the items
        /// </summary>
        /// <param name="regionList"></param>
        /// <param name="startLine"></param>
        /// <param name="endLine"></param>
        /// <returns></returns>
        private static List<CodeItem> ToHierarchy(List<CodeRegionItem> regionList, int startLine, int endLine)
        {
            return (from r in regionList
                    where r.StartLine > startLine && r.EndLine < endLine && 
                        !regionList.Any(q => IsContainedWithin(r, q) && (startLine != int.MinValue ? q.EndLine - q.StartLine < endLine - startLine : true))
                    select new CodeRegionItem
                    {
                        Name = r.Name,
                        FullName = r.Name,
                        Id = r.Name,
                        Tooltip = r.Name,
                        StartLine = r.StartLine,
                        EndLine = r.EndLine,
                        Foreground = r.Foreground,
                        BorderBrush = r.BorderBrush,
                        FontSize = r.FontSize,
                        Kind = r.Kind,
                        Members = ToHierarchy(regionList, r.StartLine, r.EndLine)
                    }).ToList<CodeItem>();
        }

        private static CodeRegionItem MapRegion(SyntaxTrivia source)
        {
            var name = "#" + source.ToString().Replace("#region ", string.Empty);
            return new CodeRegionItem
            {
                Name = name,
                FullName = name,
                Id = name,
                Tooltip = name,
                StartLine = GetStartLine(source),
                Foreground = ColorHelper.CreateSolidColorBrush(Colors.Black),
                BorderBrush = ColorHelper.CreateSolidColorBrush(Colors.DarkGray),
                FontSize = Settings.Default.Font.SizeInPoints - 2,
                Kind = CodeItemKindEnum.Region
            };
        }

        /// <summary>
        /// Add a CodeItem to the list of Regions, will recursively find the right region to add the item
        /// </summary>
        /// <param name="regions"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool AddToRegion(List<CodeRegionItem> regions, CodeItem item)
        {
            if (item?.StartLine == null) return false;
            
            foreach (var region in regions)
            {
                if (region.Kind == CodeItemKindEnum.Region)
                {
                    if (AddToRegion(region.Members, item))
                    {
                        return true;
                    }

                    if (item.StartLine >= region.StartLine && item.StartLine <= region.EndLine)
                    {
                        region.Members.Add(item);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Help add a CodeItem to a inner Region structure
        /// </summary>
        /// <param name="members"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool AddToRegion(List<CodeItem> members, CodeItem item)
        {
            foreach (var member in members)
            {
                if (member is IMembers && AddToRegion((member as IMembers).Members, item))
                {
                    return true;
                }

                if (member.Kind == CodeItemKindEnum.Region && IsContainedWithin(item, member))
                {
                    (member as CodeRegionItem).Members.Add(item);
                    return true;
                }
            }

            return false;
        }

        private static int GetStartLine(SyntaxTrivia source) =>
            source.SyntaxTree.GetLineSpan(source.Span).StartLinePosition.Line + 1;

        private static int GetEndLine(SyntaxTrivia source) =>
            source.SyntaxTree.GetLineSpan(source.Span).EndLinePosition.Line + 1;

        /// <summary>
        /// Check if item 1 is contained within item 2
        /// </summary>
        /// <param name="r1">Smaller Item</param>
        /// <param name="r2">Bigger Item</param>
        /// <returns></returns>
        private static bool IsContainedWithin(CodeItem r1, CodeItem r2) =>
            r1.StartLine > r2.StartLine && r1.EndLine < r2.EndLine;
    }
}
