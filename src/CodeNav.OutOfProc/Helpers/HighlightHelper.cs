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
    public static void HighlightCurrentItem(CodeDocumentViewModel codeDocumentViewModel,
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
            LogHelper.Log("Error highlighting current item", e);
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
            });

    /// <summary>
    /// Highlight code items that contain the current cursor position
    /// </summary>
    /// <remarks>
    /// Highlighting changes the foreground, font weight and background of a code item
    /// Deepest highlighted code item will be scrolled to, to ensure it is in view
    /// </remarks>
    /// <param name="codeDocumentViewModel">Code document</param>
    /// <param name="offset">Cursor position as a numeric offset from the start of the document</param>
    private static void Highlight(CodeDocumentViewModel codeDocumentViewModel, int offset)
        => codeDocumentViewModel
            .CodeItems
            .Flatten()
            .FilterNull()
            .Where(item => item.Span.Contains(offset))
            .ToList()
            .ForEach(item => item.IsHighlighted = true);
}
