using CodeNav.Constants;
using CodeNav.Interfaces;
using CodeNav.ViewModels;

namespace CodeNav.Helpers;

public static class SortHelper
{
    /// <summary>
    /// Apply a sort order to all needed fields on the view model
    /// </summary>
    /// <remarks>
    /// Does NOT sort the list of code items, it only updates the fields on the view model that are needed to apply the sort order.
    /// </remarks>
    /// <param name="codeDocumentViewModel"></param>
    /// <param name="sortOrder"></param>
    /// <returns></returns>
    public static CodeDocumentViewModel ApplySort(CodeDocumentViewModel codeDocumentViewModel, SortOrderEnum sortOrder)
    {
        codeDocumentViewModel.SortOrder = sortOrder;
        codeDocumentViewModel.IsSortByFileChecked = sortOrder == SortOrderEnum.SortByFile;
        codeDocumentViewModel.IsSortByNameChecked = sortOrder == SortOrderEnum.SortByName;
        return codeDocumentViewModel;
    }

    /// <summary>
    /// Sorts a list of code items according to the specified sort order.
    /// </summary>
    /// <param name="codeItems">The list of code items to sort. Cannot be null.</param>
    /// <param name="sortOrder">The sort order to apply to the code items. Determines whether items are sorted by file or by name.</param>
    /// <returns>A new list of code items sorted according to the specified sort order. If the sort order is not recognized, the
    /// original list is returned.</returns>
    public static List<CodeItem> Sort(List<CodeItem> codeItems, SortOrderEnum sortOrder)
        => sortOrder switch
        {
            SortOrderEnum.SortByFile => SortByFile(codeItems),
            SortOrderEnum.SortByName => SortByName(codeItems),
            _ => codeItems,
        };

    private static List<CodeItem> SortByName(List<CodeItem> codeItems)
    {
        codeItems = [.. codeItems.OrderBy(codeItem => codeItem.Name)];

        foreach (var item in codeItems)
        {
            if (item is IMembers membersItem)
            {
                membersItem.Members = SortByName(membersItem.Members);
            }
        }

        return codeItems;
    }

    private static List<CodeItem> SortByFile(List<CodeItem> codeItems)
    {
        codeItems = [.. codeItems.OrderBy(codeItem => codeItem.Span.Start)];

        foreach (var item in codeItems)
        {
            if (item is IMembers membersItem)
            {
                membersItem.Members = SortByFile(membersItem.Members);
            }
        }

        return codeItems;
    }
}
