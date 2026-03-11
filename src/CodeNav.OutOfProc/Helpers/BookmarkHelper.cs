using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.ViewModels;
using System.Windows;

namespace CodeNav.OutOfProc.Helpers;

public class BookmarkHelper
{
    /// <summary>
    /// Add code item to bookmark list
    /// </summary>
    /// <remarks>Used when adding item to history based on clicking a code item</remarks>
    /// <param name="item">Code item that was clicked</param>
    public static void AddItemToBookmarks(CodeDocumentViewModel codeDocumentViewModel, CodeItem? item)
    {
        if (item == null ||
            codeDocumentViewModel == null)
        {
            return;
        }

        if (!codeDocumentViewModel.BookmarkIds.Contains(item.Id))
        {
            codeDocumentViewModel.BookmarkIds.Add(item.Id);
        }

        codeDocumentViewModel.ShowFilterOnBookmarksButtonVisibility =
            codeDocumentViewModel.BookmarkIds.Any()
            ? Visibility.Visible
            : Visibility.Collapsed;

        item.IsBookmarked = true;
    }

    /// <summary>
    /// Remove code item from bookmark list
    /// </summary>
    /// <remarks>Used when adding item to history based on clicking a code item</remarks>
    /// <param name="item">Code item that was clicked</param>
    public static void RemoveItemFromBookmarks(CodeDocumentViewModel codeDocumentViewModel, CodeItem? item)
    {
        if (item == null ||
            codeDocumentViewModel == null)
        {
            return;
        }

        codeDocumentViewModel.BookmarkIds.Remove(item.Id);

        codeDocumentViewModel.ShowFilterOnBookmarksButtonVisibility =
            codeDocumentViewModel.BookmarkIds.Any()
            ? Visibility.Visible
            : Visibility.Collapsed;

        item.IsBookmarked = false;
    }

    /// <summary>
    /// Apply bookmark indicator to all code items in the bookmark list
    /// </summary>
    /// <remarks>Uses Flatten() to do this recursively for child code items</remarks>
    /// <param name="codeDocumentViewModel">Document holding all code items</param>
    public static void ApplyBookmarkIndicator(CodeDocumentViewModel codeDocumentViewModel)
    {
        // Clear old indicators
        codeDocumentViewModel
            .CodeItems
            .Flatten()
            .Where(codeItem => codeItem != null)
            .ToList()
            .ForEach(codeItem => codeItem.IsBookmarked = false);

        // Apply new indicators
        codeDocumentViewModel
            .BookmarkIds
            .ForEach(bookmarkId =>
            {
                var codeItem = codeDocumentViewModel.CodeItems
                    .Flatten()
                    .Where(codeItem => codeItem != null)
                    .FirstOrDefault(codeItem => codeItem.Id == bookmarkId);

                if (codeItem != null)
                {
                    codeItem.IsBookmarked = true;
                }
            });
    }

    /// <summary>
    /// Delete all bookmarks
    /// </summary>
    /// <param name="item">Code item on which the context menu was invoked</param>
    public static void ClearBookmarks(CodeDocumentViewModel? codeDocumentViewModel)
    {
        if (codeDocumentViewModel == null)
        {
            return;
        }

        codeDocumentViewModel.BookmarkIds.Clear();

        codeDocumentViewModel.ShowFilterOnBookmarksButtonVisibility = Visibility.Collapsed;

        ApplyBookmarkIndicator(codeDocumentViewModel);
    }
}