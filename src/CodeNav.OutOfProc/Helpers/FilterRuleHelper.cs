using CodeNav.OutOfProc.Constants;
using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.ViewModels;
using System.Windows;

namespace CodeNav.OutOfProc.Helpers;

public static class FilterRuleHelper
{
    /// <summary>
    /// Get the last matching filter rule for a given code item
    /// </summary>
    /// <param name="filterRules">List of filter rules</param>
    /// <param name="codeItem">Code item</param>
    /// <returns>Matching filter rule</returns>
    public static FilterRuleViewModel? GetFilterRule(IEnumerable<FilterRuleViewModel> filterRules, CodeItem codeItem)
    {
        // Get the most specific filter rule for the item
        var filterRule = filterRules
            .Where(filterRule => filterRule.Access == codeItem.Access ||
                                 filterRule.Access == CodeItemAccessEnum.All)
            .Where(filterRule => filterRule.Kind == codeItem.Kind ||
                                 filterRule.Kind == CodeItemKindEnum.All)
            .Where(filterRule => filterRule.IsEmpty == null ||
                                 filterRule.IsEmpty == IsEmpty(codeItem))
            .LastOrDefault();

        return filterRule;
    }

    public static FilterRuleViewModel? GetFilterRule(CodeDocumentViewModel codeDocumentViewModel, CodeItemKindEnum codeItemKind)
    {
        // Check if we have any filters at all
        if (codeDocumentViewModel?.FilterRules?.Any() != true)
        {
            return null;
        }

        // Get the most specific filter rule for the item
        var filterRule = codeDocumentViewModel?.FilterRules
            .LastOrDefault(filterRule => filterRule.Kind == codeItemKind || filterRule.Kind == CodeItemKindEnum.All);

        return filterRule;
    }

    public static IEnumerable<CodeItem> ApplyFilterRules(
        CodeDocumentViewModel codeDocumentViewModel,
        IEnumerable<CodeItem> codeItems,
        IEnumerable<FilterRuleViewModel> filterRules)
    {
        try
        {
            codeItems
                .Flatten()
                .FilterNull()
                .ToList()
                .ForEach(codeItem =>
                {
                    if (codeItem is IMembers hasMembersItem &&
                        hasMembersItem.Members.Any())
                    {
                        ApplyFilterRules(codeDocumentViewModel, hasMembersItem.Members, filterRules);
                    }

                    var filterRule = GetFilterRule(filterRules, codeItem);

                    codeItem.IsItalic = filterRule?.Italic == true;

                    if (filterRule != null &&
                        filterRule.FontScale != 100)
                    {
                        codeItem.FontSize = 9 * (filterRule.FontScale / 100d);
                    }
                });
        }
        catch (Exception e)
        {
            _ = LogHelper.LogException(codeDocumentViewModel, "Error during applying filter rules", e);
        }

        return codeItems;

    }

    /// <summary>
    /// Determines whether the specified code item contains no visible members.
    /// </summary>
    /// <remarks>A code item that does not implement IMembers or has a null Members collection is considered
    /// not empty. Only members with Visibility.Visible are counted as visible.</remarks>
    /// <param name="codeItem">The code item to evaluate for visible members. Must implement the IMembers interface to be considered.</param>
    /// <returns>true if the code item has no members with Visibility.Visible; otherwise, false.</returns>
    private static bool IsEmpty(CodeItem codeItem)
    {
        if (codeItem is not IMembers membersCodeItem)
        {
            return false;
        }

        if (membersCodeItem.Members == null)
        {
            return false;
        }

        return !membersCodeItem.Members.Any(member => member.Visibility == Visibility.Visible);
    }
}
