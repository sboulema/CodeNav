using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.ViewModels;

namespace CodeNav.OutOfProc.Helpers;

public static class HighlightHelper
{
    /// <summary>
    /// Highlight code items that contain the current line number
    /// </summary>
    /// <param name="codeDocumentViewModel">Code document</param>
    /// <param name="offset">Cursor position as a numeric offset from the start of the document</param>
    public static async Task HighlightCurrentItem(CodeDocumentViewModel codeDocumentViewModel,
        int offset)
    {
        if (codeDocumentViewModel == null)
        {
            return;
        }

        try
        {
            UnHighlight(codeDocumentViewModel);
            Highlight(codeDocumentViewModel, offset);
        }
        catch (Exception e)
        {
            await LogHelper.LogException(codeDocumentViewModel, "Error highlighting current item", e);
        }
    }

    /// <summary>
    /// Remove highlight from all code items
    /// </summary>
    /// <remarks>Will restore bookmark foreground color when unhighlighting a bookmarked item</remarks>
    /// <param name="codeDocumentViewModel">Code document</param>
    public static void UnHighlight(CodeDocumentViewModel codeDocumentViewModel)
        => codeDocumentViewModel.CodeItems
            .Flatten()
            .FilterNull()
            .ToList()
            .ForEach(item =>
            {
                item.IsHighlighted = false;
                item.IsScrollTarget = false;
            });

    /// <summary>
    /// Highlight code items that contain the current cursor position
    /// </summary>
    /// <remarks>
    /// Highlighting changes the foreground, font weight and background of a code item
    /// The deepest highlighted code item is flagged with <see cref="CodeItem.IsScrollTarget"/>,
    /// so it can be scrolled to, to ensure it is in view
    /// </remarks>
    /// <param name="codeDocumentViewModel">Code document</param>
    /// <param name="offset">Cursor position as a numeric offset from the start of the document</param>
    private static void Highlight(CodeDocumentViewModel codeDocumentViewModel, int offset)
    {
        var highlightedItems = codeDocumentViewModel
            .CodeItems
            .Flatten()
            .FilterNull()
            .Where(item => item.Span.Contains(offset))
            .ToList();

        highlightedItems.ForEach(item => item.IsHighlighted = true);

        var deepestItem = highlightedItems
            .OrderBy(item => item.Span.Length)
            .FirstOrDefault();

        if (deepestItem != null)
        {
            deepestItem.IsScrollTarget = true;
        }
    }
}
