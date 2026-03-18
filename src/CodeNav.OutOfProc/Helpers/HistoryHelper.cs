using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.VisualStudio.Extensibility.Editor;
using System.Windows;

namespace CodeNav.OutOfProc.Helpers;

public class HistoryHelper
{
    private const int MaxHistoryItems = 5;

    /// <summary>
    /// Add code item to history items based on text edits
    /// </summary>
    /// <remarks>Used when adding item to history based on text changes</remarks>
    /// <param name="model">Document holding all code items</param>
    /// <param name="textEdits">List of text edits made</param>
    public static async Task AddItemToHistory(CodeDocumentViewModel codeDocumentViewModel, IEnumerable<TextEdit> textEdits)
    {
        foreach (var textEdit in textEdits)
        {
            try
            {
                var item = codeDocumentViewModel
                    .CodeItems
                    .Flatten()
                    .FilterNull()
                    .Where(item => item is not IMembers)
                    .FirstOrDefault(item => item.Span.Contains(textEdit.Range.Start));

                AddItemToHistory(item);
            }
            catch (Exception e)
            {
                await LogHelper.LogException(codeDocumentViewModel, "Error adding item to history", e);
            }
        }
    }

    /// <summary>
    /// Add code item to history items based on its code item
    /// </summary>
    /// <remarks>Used when adding item to history based on clicking a code item</remarks>
    /// <param name="item">Code item that was clicked</param>
    public static void AddItemToHistory(CodeItem? item)
    {
        if (item == null)
        {
            return;
        }

        var model = item.CodeDocumentViewModel;

        if (model == null)
        {
            return;
        }

        if (model.CodeDocumentService?.SettingsDialogData.ShowHistoryIndicators == false)
        {
            return;
        }

        // Remove all entries with item id to prevent duplicates
        model.HistoryItemIds
            .RemoveAll(id => id == item.Id);

        // Insert the new item id
        model.HistoryItemIds.Insert(0, item.Id);

        // Only keep the max number of item ids
        model.HistoryItemIds = [.. model.HistoryItemIds.Take(MaxHistoryItems)];

        ApplyHistoryIndicator(model);
    }

    /// <summary>
    /// Apply history indicator to all code items in the history list
    /// </summary>
    /// <remarks>Uses Flatten() to do this recursively for child code items</remarks>
    /// <param name="model">Document holding all code items</param>
    public static void ApplyHistoryIndicator(CodeDocumentViewModel model)
    {
        // Clear old indicators
        model
            .CodeItems
            .Flatten()
            .Where(item => item != null)
            .ToList()
            .ForEach(item => item.StatusMonikerVisibility = Visibility.Collapsed);

        // Apply new indicators
        var rankedHistoryItemIds = model
            .HistoryItemIds
            .Where(id => !string.IsNullOrEmpty(id))
            .Select((historyItemId, index) => (historyItemId, index));

        foreach (var (historyItemId, index) in rankedHistoryItemIds)
        {
            var codeItem = model.CodeItems
                .Flatten()
                .Where(item => item != null)
                .FirstOrDefault(item => item.Id == historyItemId);

            if (codeItem == null)
            {
                continue;
            }

            ApplyHistoryIndicator(codeItem, index);
        }
    }

    /// <summary>
    /// Apply history indicator to a single code item
    /// </summary>
    /// <remarks>Index determines color and opacity of the history indicator</remarks>
    /// <param name="item">Code item in history list</param>
    /// <param name="index">Index in history list</param>
    private static void ApplyHistoryIndicator(CodeItem item, int index = 0)
    {
        item.StatusMonikerVisibility = Visibility.Visible;

        // Show the icon in grayscale if it is not the most recent history item
        item.StatusGrayscale = index > 0;

        item.StatusOpacity = GetOpacity(index);
    }

    /// <summary>
    /// Get opacity based on history list index
    /// </summary>
    /// <param name="index">Index in history list</param>
    /// <remarks>
    /// 0: latest history item => 100%
    /// 1-2: => 90%
    /// 3-4: => 70%
    /// </remarks>
    /// <returns>Double between 0 and 1</returns>
    private static double GetOpacity(int index)
        => index switch
        {
            0 => 1,
            1 or 2 => 0.9,
            3 or 4 => 0.7,
            _ => 1,
        };

    /// <summary>
    /// Delete all history item indicators
    /// </summary>
    /// <param name="item">Code item on which the context menu was invoked</param>
    public static void ClearHistory(CodeDocumentViewModel? codeDocumentViewModel)
    {
        if (codeDocumentViewModel == null)
        {
            return;
        }

        codeDocumentViewModel.HistoryItemIds.Clear();

        ApplyHistoryIndicator(codeDocumentViewModel);
    }
}