using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Mappers;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeNav.OutOfProc.Languages.VisualBasic.Mappers;

public static class RegionMapper
{
    /// <summary>
    /// Find all regions in a file and get their start and end span.
    /// </summary>
    /// <param name="tree">Syntax tree for the given file.</param>
    /// <param name="span">Span in which to search for regions.</param>
    /// <param name="codeDocumentViewModel">Code document view model.</param>
    /// <returns>Flat list of regions.</returns>
    public static List<CodeRegionItem> MapRegions(
        SyntaxTree tree,
        TextSpan span,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var regionList = new List<CodeRegionItem>();

        if (tree == null)
        {
            return regionList;
        }

        // Check if regions should be ignored based on the filter rules.
        var filterRule = FilterRuleHelper.GetFilterRule(
            codeDocumentViewModel,
            CodeItemKindEnum.Region);

        if (filterRule?.Ignore == true)
        {
            return regionList;
        }

        var root = tree.GetRoot();

        // Find all region start trivia.
        var regionStarts = root
            .DescendantTrivia()
            .Where(syntaxTrivia =>
                syntaxTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            .Where(syntaxTrivia =>
                span.Contains(syntaxTrivia.Span));

        regionList.AddRange(
            regionStarts.Select(regionStart =>
                MapRegion(regionStart, codeDocumentViewModel)));

        // Find all region end trivia.
        var regionEnds = root
            .DescendantTrivia()
            .Where(syntaxTrivia =>
                syntaxTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            .Where(syntaxTrivia =>
                span.Contains(syntaxTrivia.Span));

        // Match a region start with the closest region end.
        foreach (var regionEnd in regionEnds)
        {
            var region = regionList
                .LastOrDefault(regionStart =>
                    regionStart.Span.Start < regionEnd.Span.Start &&
                    regionStart.Span.End == regionStart.Span.Start);

            if (region == null)
            {
                continue;
            }

            region.Span = new(
                region.Span.Start,
                regionEnd.Span.End - region.Span.Start);

            region.OutlineSpan = new(
                region.OutlineSpan.Start,
                regionEnd.Span.End - region.Span.Start);
        }

        return ToHierarchy(regionList, span);
    }

    /// <summary>
    /// Transform the flat list of regions into a nested hierarchy.
    /// </summary>
    private static List<CodeRegionItem> ToHierarchy(
        List<CodeRegionItem> regionList,
        TextSpan textSpan)
    {
        var nestedRegions = new List<CodeRegionItem>();

        foreach (var region in regionList)
        {
            if (!StrictContains(textSpan, region.Span) ||
                regionList.Any(otherBiggerRegion =>
                    StrictContains(
                        otherBiggerRegion.Span,
                        region.Span) &&
                    otherBiggerRegion.Span.Length < textSpan.Length))
            {
                continue;
            }

            region.Members =
                [.. ToHierarchy(regionList, region.Span).Cast<CodeItem>()];

            nestedRegions.Add(region);
        }

        return nestedRegions;
    }

    private static CodeRegionItem MapRegion(
        SyntaxTrivia regionStart,
        CodeDocumentViewModel codeDocumentViewModel)
    {
        var name = MapRegionName(regionStart);

        return new()
        {
            Name = name,
            FullName = name,
            Id = name,
            Tooltip = name,
            Kind = CodeItemKindEnum.Region,
            Span = new(
                regionStart.Span.Start,
                0),
            OutlineSpan = new(
                regionStart.Span.Start,
                0),
            IdentifierSpan = MapIdentifierSpan(regionStart),
            Moniker = IconMapper.MapMoniker(
                CodeItemKindEnum.Region,
                CodeItemAccessEnum.Unknown),
            CodeDocumentViewModel = codeDocumentViewModel,
        };
    }

    private static TextSpan MapIdentifierSpan(
        SyntaxTrivia regionStart)
    {
        return regionStart.Span;
    }

    private static string MapRegionName(
        SyntaxTrivia regionStart)
    {
        const string defaultRegionName = "Region";
        const string regionKeyword = "#Region";

        var syntaxNode = regionStart.GetStructure();

        if (syntaxNode is not RegionDirectiveTriviaSyntax regionSyntax)
        {
            return defaultRegionName;
        }

        var text = regionSyntax.ToString();

        if (!text.StartsWith(
                regionKeyword,
                StringComparison.OrdinalIgnoreCase))
        {
            return defaultRegionName;
        }

        var name = text[regionKeyword.Length..]
            .Trim()
            .Trim('"');

        return string.IsNullOrEmpty(name)
            ? defaultRegionName
            : name;
    }

    /// <summary>
    /// Add a code item to the list of regions, recursively finding
    /// the correct region.
    /// </summary>
    public static bool AddToRegion(
        List<CodeRegionItem> regions,
        CodeItem codeItem)
    {
        if (codeItem?.Span == null)
        {
            return false;
        }

        foreach (var region in regions)
        {
            if (region?.Kind != CodeItemKindEnum.Region)
            {
                continue;
            }

            if (AddToRegion(region.Members, codeItem))
            {
                return true;
            }

            if (codeItem.Span.Start >= region.Span.Start &&
                codeItem.Span.Start <= region.Span.End)
            {
                region.Members.Add(codeItem);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Help add a code item to an inner region structure.
    /// </summary>
    private static bool AddToRegion(
        List<CodeItem> members,
        CodeItem codeItem)
    {
        foreach (var member in members)
        {
            if (member == null)
            {
                continue;
            }

            if (member is IMembers memberItem &&
                AddToRegion(memberItem.Members, codeItem))
            {
                return true;
            }

            if (member is CodeRegionItem regionItem &&
                member.Span.Contains(codeItem.Span))
            {
                regionItem.Members.Add(codeItem);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a small span falls completely within
    /// a bigger span without touching its boundaries.
    /// </summary>
    public static bool StrictContains(
        TextSpan bigSpan,
        TextSpan smallSpan)
        => smallSpan.Start > bigSpan.Start &&
           smallSpan.End < bigSpan.End;

    /// <summary>
    /// Check if a code item is part of any region.
    /// </summary>
    public static bool IsPartOfRegion(
        IEnumerable<CodeRegionItem> regions,
        CodeItem codeItem)
        => regions.Any(region =>
            region.Span.Contains(codeItem.Span));

    public static CodeRegionItem? GetRegion(
        IEnumerable<CodeItem> regions,
        CodeItem codeItem)
    {
        foreach (var region in regions)
        {
            if (region.Kind != CodeItemKindEnum.Region)
            {
                continue;
            }

            if (!region.Span.Contains(codeItem.Span))
            {
                continue;
            }

            // Try to find a more specific nested region first.
            if (region is IMembers regionMembersItem)
            {
                var nestedMatch = GetRegion(
                    regionMembersItem.Members,
                    codeItem);

                return nestedMatch ??
                    region as CodeRegionItem;
            }

            return region as CodeRegionItem;
        }

        return null;
    }

    /// <summary>
    /// Add regions to the given list of members, but only if a region
    /// with the same ID is not already present somewhere within those members.
    /// </summary>
    public static void AddRegionsIfNotPresent(
        List<CodeItem> members,
        List<CodeRegionItem> regions)
    {
        foreach (var region in regions)
        {
            var alreadyPresent = members
                .Flatten()
                .FilterNull()
                .Any(item => item.Id == region?.Id);

            if (!alreadyPresent)
            {
                members.AddIfNotNull(region);
            }
        }
    }
}