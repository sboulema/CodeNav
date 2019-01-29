using CodeNav.Helpers;
using CodeNav.Models;
using CodeNav.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using VisualBasic = Microsoft.CodeAnalysis.VisualBasic;
using VisualBasicSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CodeNav.Mappers
{
    public static class RegionMapper
    {
        /// <summary>
        /// Find all regions in a file and get there start and end line
        /// </summary>
        /// <param name="tree">SyntaxTree for the given file</param>
        /// <param name="span">Start and end line in which we search for regions</param>
        /// <returns>Flat list of regions</returns>
        public static List<CodeRegionItem> MapRegions(SyntaxTree tree, TextSpan span)
        {
            var regionList = new List<CodeRegionItem>();

            if (tree == null) return regionList;

            var root = tree.GetRoot();
            
            // Find all start points of regions
            foreach (var regionDirective in root.DescendantTrivia()
                .Where(i => (i.RawKind == (int)SyntaxKind.RegionDirectiveTrivia ||
                             i.RawKind == (int)VisualBasic.SyntaxKind.RegionDirectiveTrivia) && 
                             span.Contains(i.Span)))
            {
                regionList.Add(MapRegion(regionDirective));
            }

            if (!regionList.Any()) return regionList;

            // Find all matching end points of regions
            foreach (var endRegionDirective in root.DescendantTrivia()
                .Where(j => (j.RawKind == (int)SyntaxKind.EndRegionDirectiveTrivia ||
                             j.RawKind == (int)VisualBasic.SyntaxKind.EndRegionDirectiveTrivia) && 
                             span.Contains(j.Span)))
            {              
                var reg = regionList.LastOrDefault(x => x.StartLine < GetStartLine(endRegionDirective) && x.EndLine == 0);
                if (reg != null)
                {
                    reg.EndLine = GetEndLine(endRegionDirective);
                }             
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
                        ForegroundColor = r.ForegroundColor,
                        BorderColor = r.BorderColor,
                        FontSize = r.FontSize,
                        Kind = r.Kind,
                        Span = r.Span,
                        Members = ToHierarchy(regionList, r.StartLine, r.EndLine)
                    }).ToList<CodeItem>();
        }

        private static CodeRegionItem MapRegion(SyntaxTrivia source)
        {
            var name = MapRegionName(source);

            return new CodeRegionItem
            {
                Name = name,
                FullName = name,
                Id = name,
                Tooltip = name,
                StartLine = GetStartLine(source),
                ForegroundColor = Colors.Black,
                BorderColor = Colors.DarkGray,
                FontSize = Settings.Default.Font.SizeInPoints - 2,
                Kind = CodeItemKindEnum.Region,
                Span = source.Span
            };
        }

        private static string MapRegionName(SyntaxTrivia source)
        {
            const string defaultRegionName = "Region";
            var syntaxNode = source.GetStructure();
            var name = "#";

            switch (LanguageHelper.GetLanguage(syntaxNode.Language))
            {
                case LanguageEnum.CSharp:
                    var endDirectiveToken = (syntaxNode as RegionDirectiveTriviaSyntax).EndOfDirectiveToken;
                    if (endDirectiveToken.HasLeadingTrivia)
                    {
                        name += endDirectiveToken.LeadingTrivia.First().ToString();
                    }
                    else
                    {
                        name += defaultRegionName;
                    }                   
                    break;
                case LanguageEnum.VisualBasic:
                    name += (syntaxNode as VisualBasicSyntax.RegionDirectiveTriviaSyntax).Name.ValueText;
                    break;
                default:
                    name += defaultRegionName;
                    break;
            }

            return name;
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
        /// Help add a CodeItem to an inner Region structure
        /// </summary>
        /// <param name="members"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool AddToRegion(List<CodeItem> members, CodeItem item)
        {
            foreach (var member in members)
            {
                if (member == null) continue;

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
