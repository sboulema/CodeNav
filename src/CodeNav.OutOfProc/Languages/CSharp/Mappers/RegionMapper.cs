using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.OutOfProc.Languages.CSharp.Mappers;

public static class RegionMapper
{
    /// <summary>
    /// Find all regions in a file and get there start and end line
    /// </summary>
    /// <param name="tree">SyntaxTree for the given file</param>
    /// <param name="span">Start and end line in which we search for regions</param>
    /// <returns>Flat list of regions</returns>
    public static List<CodeRegionItem> MapRegions(SyntaxTree tree, TextSpan span, CodeDocumentViewModel codeDocumentViewModel)
    {
        var regionList = new List<CodeRegionItem>();

        if (tree == null)
        {
            return regionList;
        }

        // Check if we should ignore regions based on the filter rules,
        // if so return an empty list of regions,
        // so that members will not be mapped to a region
        var filterRule = FilterRuleHelper.GetFilterRule(codeDocumentViewModel, CodeItemKindEnum.Region);

        if (filterRule?.Ignore == true)
        {
            return regionList;
        }

        var root = tree.GetRoot();
        
        // Find all region start trivia
        var regionStarts = root
            .DescendantTrivia()
            .Where(syntaxTrivia => syntaxTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            .Where(syntaxTrivia => span.Contains(syntaxTrivia.Span));

        regionList.AddRange(regionStarts.Select(MapRegion));

        // Find all region end trivia
        var regionEnds = root
            .DescendantTrivia()
            .Where(syntaxTrivia => syntaxTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            .Where(syntaxTrivia => span.Contains(syntaxTrivia.Span));

        // Match a region start with the closest region end
        foreach (var regionEnd in regionEnds)
        {
            var region = regionList
                .LastOrDefault(regionStart => regionStart.Span.Start < regionEnd.Span.Start &&
                    regionStart.Span.End == regionStart.Span.Start);

            if (region == null)
            {
                continue;
            }

            region.Span = new(region.Span.Start, regionEnd.Span.End - region.Span.Start);
        }

        var regions = ToHierarchy(regionList, span);

        return regions;
    }

    /// <summary>
    /// Transform the flat list of Regions to a nested List based on the span start and span end of the items
    /// </summary>
    /// <param name="regionList"></param>
    /// <param name="startLine"></param>
    /// <param name="endLine"></param>
    /// <returns></returns>
    private static List<CodeRegionItem> ToHierarchy(List<CodeRegionItem> regionList, TextSpan textSpan)
    {
        var nestedRegions = new List<CodeRegionItem>();

        foreach (var region in regionList)
        {
            if (StrictContains(textSpan, region.Span) && 
                !regionList.Any(otherBiggerRegion =>
                    StrictContains(otherBiggerRegion.Span, region.Span) &&
                    otherBiggerRegion.Span.Length < textSpan.Length))
            {
                region.Members = [.. ToHierarchy(regionList, region.Span).Cast<CodeItem>()];

                nestedRegions.Add(region);
            }
        }

        return nestedRegions;
    }

    private static CodeRegionItem MapRegion(SyntaxTrivia regionStart)
    {
        var name = MapRegionName(regionStart);

        return new()
        {
            Name = name,
            FullName = name,
            Id = name,
            Tooltip = name,
            Kind = CodeItemKindEnum.Region,
            Span = new(regionStart.Span.Start, 0),
            Moniker = IconMapper.MapMoniker(CodeItemKindEnum.Region, CodeItemAccessEnum.Unknown),
        };
    }

    private static string MapRegionName(SyntaxTrivia source)
    {
        const string defaultRegionName = "Region";
        var syntaxNode = source.GetStructure();
        var name = "#";

        if (syntaxNode is not RegionDirectiveTriviaSyntax regionSyntax)
        {
            name += defaultRegionName;
            return name;
        }

        var endDirectiveToken = regionSyntax.EndOfDirectiveToken;
        if (endDirectiveToken.HasLeadingTrivia)
        {
            name += endDirectiveToken.LeadingTrivia.First().ToString();
        }
        else
        {
            name += defaultRegionName;
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
        if (item?.Span == null)
        {
            return false;
        }

        foreach (var region in regions)
        {
            if (region?.Kind != CodeItemKindEnum.Region)
            {
                continue;
            }

            if (AddToRegion(region.Members, item))
            {
                return true;
            }

            if (item.Span.Start >= region.Span.Start &&
                item.Span.Start <= region.Span.End)
            {
                region.Members.Add(item);
                return true;
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
            if (member == null)
            {
                continue;
            }

            if (member is IMembers memberItem && AddToRegion(memberItem.Members, item))
            {
                return true;
            }

            if (member is CodeRegionItem regionItem &&
                member.Span.Contains(item.Span))
            {
                regionItem.Members.Add(item);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether <paramref name="smallSpan"/> falls completely within <paramref name="bigSpan"/>.
    /// </summary>
    /// <param name="smallSpan">
    /// The span to check.
    /// </param>
    /// <param name="bigSpan">
    /// The span to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified span falls completely within this span without overlapping start or end, otherwise <c>false</c>.
    /// </returns>
    public static bool StrictContains(TextSpan bigSpan, TextSpan smallSpan)
        => smallSpan.Start > bigSpan.Start &&
           smallSpan.End < bigSpan.End;
}
