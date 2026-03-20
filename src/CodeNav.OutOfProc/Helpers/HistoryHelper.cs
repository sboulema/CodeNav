using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.VisualStudio.Extensibility.Editor;
using System.Windows;

namespace CodeNav.OutOfProc.Helpers;

public static class HistoryHelper
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
    /// <param name="codeItem">Code item that was clicked</param>
    public static void AddItemToHistory(CodeItem? codeItem)
    {
        if (codeItem == null)
        {
            return;
        }

        var codeDocumentViewModel = codeItem.CodeDocumentViewModel;

        if (codeDocumentViewModel == null)
        {
            return;
        }

        if (codeDocumentViewModel.CodeDocumentService?.SettingsDialogData.ShowHistoryIndicators == false)
        {
            return;
        }

        lock (codeDocumentViewModel.HistoryLock)
        {
            // Remove all entries with item id to prevent duplicates
            codeDocumentViewModel.HistoryItemIds
                .RemoveAll(id => id == codeItem.Id);

            // Insert the new item id
            codeDocumentViewModel.HistoryItemIds
                .Insert(0, codeItem.Id);

            // Only keep the max number of item ids
            codeDocumentViewModel.HistoryItemIds = [.. codeDocumentViewModel.HistoryItemIds.Take(MaxHistoryItems)];
        }

        ApplyHistoryIndicator(codeDocumentViewModel);
    }

    /// <summary>
    /// Apply history indicator to all code items in the history list
    /// </summary>
    /// <remarks>Uses Flatten() to do this recursively for child code items</remarks>
    /// <param name="codeDocumentViewModel">Document holding all code items</param>
    public static void ApplyHistoryIndicator(CodeDocumentViewModel codeDocumentViewModel)
    {
        // Clear old indicators
        codeDocumentViewModel
            .CodeItems
            .Flatten()
            .Where(item => item != null)
            .ToList()
            .ForEach(item => item.StatusMonikerVisibility = Visibility.Collapsed);

        IEnumerable<(string, int)> rankedHistoryItemIds = [];

        lock (codeDocumentViewModel.HistoryLock)
        {
            // Apply new indicators
            rankedHistoryItemIds = codeDocumentViewModel
                .HistoryItemIds
                .Where(id => !string.IsNullOrEmpty(id))
                .Select((historyItemId, index) => (historyItemId, index));
        }

        foreach (var (historyItemId, index) in rankedHistoryItemIds)
        {
            var codeItem = codeDocumentViewModel.CodeItems
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