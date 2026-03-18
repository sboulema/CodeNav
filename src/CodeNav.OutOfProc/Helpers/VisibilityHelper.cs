using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.ViewModels;
using System.Windows;

namespace CodeNav.OutOfProc.Helpers;

public static class VisibilityHelper
{
    /// <summary>
    /// Loop through all code items and set if the item should be visible or not.
    /// </summary>
    /// <remarks>
    /// - Check if the code item should be visible based on the fitler rules
    /// - Check if the code item's name contains the filter text
    /// </remarks>
    /// <param name="document">List of code items</param>
    /// <param name="filterText">Text that should be contained in the code items name</param>
    public static IEnumerable<CodeItem> SetCodeItemVisibility(
        CodeDocumentViewModel codeDocumentViewModel,
        IEnumerable<CodeItem> codeItems,
        IEnumerable<FilterRuleViewModel> filterRules,
        string filterText = "",
        IEnumerable<string>? bookmarkIds = null)
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
                        SetCodeItemVisibility(codeDocumentViewModel, hasMembersItem.Members, filterRules, filterText, bookmarkIds);
                    }

                    codeItem.Visibility = ShouldBeVisible(codeItem, filterRules, filterText, bookmarkIds)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                    codeItem.Opacity = ShouldHaveOpacity(codeItem, filterRules);

                    if (codeItem is CodeNamespaceItem codeNamespaceItem)
                    {
                        codeNamespaceItem.IgnoreVisibility = ShouldBeVisibleByIgnored(codeItem, filterRules);
                    }
                });
        }
        catch (Exception e)
        {
            _ = LogHelper.LogException(codeDocumentViewModel, "Error during setting visibility", e);
        }

        return codeItems;
    }

    /// <summary>
    /// Determine if a code item should be visible
    /// </summary>
    /// <remarks>
    /// - Check if the code item should be visible based on the fitler rules
    /// - Check if the code item's name contains the filter text
    /// </remarks>
    /// <param name="codeItem">Code item that is checked</param>
    /// <param name="filterText">Text filter</param>
    /// <returns></returns>
    private static bool ShouldBeVisible(
        CodeItem codeItem,
        IEnumerable<FilterRuleViewModel> filterRules,
        string filterText = "",
        IEnumerable<string>? bookmarkIds = null)
    {
        var visible = true;

        // Check filter rules
        var filterRule = FilterRuleHelper.GetFilterRule(filterRules, codeItem);

        if (filterRule?.Hide == true)
        {
            return false;
        }

        // Check filter text
        if (!string.IsNullOrEmpty(filterText))
        {
            visible = visible &&
                codeItem.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase);
        }

        // If we filter on bookmarks, only show items that are in the bookmark list
        if (bookmarkIds?.Any() == true &&
            bookmarkIds.Contains(codeItem.Id) == false)
        {
            visible = false;
        }

        // If an item has any visible members, it should be visible.
        if ((codeItem as IMembers)?.Members.Any(m => m.Visibility == Visibility.Visible) == true)
        {
            visible = true;
        }

        return visible;
    }

    /// <summary>
    /// Determine if a code item should be visible based on the ignore filter
    /// </summary>
    /// <remarks>
    /// - Only for namespaces, Ignore for implemented interfaces and regions is handled elsewhere
    /// </remarks>
    /// <param name="filterRules"></param>
    /// <param name="codeItem"></param>
    /// <returns></returns>
    public static Visibility ShouldBeVisibleByIgnored(
        CodeItem codeItem,
        IEnumerable<FilterRuleViewModel> filterRules)
    {
        var filterRule = FilterRuleHelper.GetFilterRule(filterRules, codeItem);

        return filterRule?.Ignore == true
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public static double ShouldHaveOpacity(
        CodeItem codeItem,
        IEnumerable<FilterRuleViewModel> filterRules)
    {
        var filterRule = FilterRuleHelper.GetFilterRule(filterRules, codeItem);

        if (filterRule == null)
        {
            return 1;
        }

        if (filterRule.Opacity < 0 || filterRule.Opacity > 100)
        {
            return 1;
        }

        return filterRule.Opacity / 100d;
    }
}
