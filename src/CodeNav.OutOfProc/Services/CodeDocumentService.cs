using CodeNav.OutOfProc.Dialogs.FilterDialog;
using CodeNav.OutOfProc.Dialogs.SettingsDialog;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Languages.CSharp.Mappers;
using CodeNav.OutOfProc.Models;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Windows;

namespace CodeNav.OutOfProc.Services;

public class CodeDocumentService(
    OutputWindowService logService,
    OutliningService outliningService,
    WindowFrameService windowFrameService)
{
    /// <summary>
    /// DataContext for the tool window.
    /// </summary>
    public CodeDocumentViewModel CodeDocumentViewModel { get; set; } = new();

    /// <summary>
    /// DataContext for the settings dialog.
    /// </summary>
    public SettingsDialogData SettingsDialogData { get; set; } = new();

    /// <summary>
    /// DataContext for the filter dialog.
    /// </summary>
    public FilterDialogData FilterDialogData { get; set; } = new();

    public GlobalSettings? GlobalSettings { get; set; }

    public OutputWindowService LogService => logService;

    public OutliningService OutliningService => outliningService;

    public ToolWindow? ToolWindow { get; set; }

    public async Task<CodeDocumentViewModel> UpdateCodeDocumentViewModel(
        VisualStudioExtensibility? extensibility,
        string? filePath,
        string? text,
        CancellationToken cancellationToken)
    {
        try
        {
            if (extensibility == null ||
                string.IsNullOrEmpty(filePath) ||
                string.IsNullOrEmpty(text))
            {
                return CodeDocumentViewModel;
            }

            // Show loading item while we process the document
            var loadingCancellationTokenSource = new CancellationTokenSource();
            var loadingCancellationToken = loadingCancellationTokenSource.Token;

            _ = PlaceholderHelper.CreateLoadingItem(
                CodeDocumentViewModel,
                500,
                loadingCancellationToken);

            // Get the new list of code items
            var codeItems = await DocumentMapper.MapDocument(
                text,
                filePath,
                CodeDocumentViewModel,
                extensibility,
                cancellationToken);

            await logService.WriteInfo(filePath, $"Found '{codeItems.Count}' code items");

            // Getting the new code items is done, cancel creating a loading placeholder
            await loadingCancellationTokenSource.CancelAsync();

            // Set properties on the CodeDocumentViewModel that are needed for other features
            CodeDocumentViewModel.CodeDocumentService = this;
            CodeDocumentViewModel.FilePath = filePath ?? string.Empty;

            if (!codeItems.Any())
            {
                CodeDocumentViewModel.CodeItems = PlaceholderHelper.CreateNoCodeItemsFound();

                // No code items found, hide the tool window after showing the "No code items found" message
                await HideToolWindow(cancellationToken);
                
                return CodeDocumentViewModel;
            }

            // Code items were found, make sure the tool window is visible
            await ShowToolWindow(cancellationToken);

            // Sort the list of code items,
            // And update the DataContext for the tool window
            CodeDocumentViewModel.CodeItems = SortHelper.Sort(codeItems, CodeDocumentViewModel.SortOrder);

            await logService.WriteInfo(filePath, $"Sorted code items on '{CodeDocumentViewModel.SortOrder}'");

            // Apply highlights
            HighlightHelper.UnHighlight(CodeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Remove highlight from all code items");

            // Apply current visibility settings to the document
            VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel, CodeDocumentViewModel.CodeItems, CodeDocumentViewModel.FilterRules);

            await logService.WriteInfo(filePath, $"Set code item visibility");

            // Apply filter rules
            FilterRuleHelper.ApplyFilterRules(CodeDocumentViewModel, CodeDocumentViewModel.CodeItems, CodeDocumentViewModel.FilterRules);

            await logService.WriteInfo(filePath, $"Set code item filter rules");

            // Apply history items
            HistoryHelper.ApplyHistoryIndicator(CodeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Apply history indicators");

            // Apply bookmarks
            BookmarkHelper.ApplyBookmarkIndicator(CodeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Apply bookmark indicators");

            // Apply outlining
            await OutliningService.SubscribeToRegionEvents(CodeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Apply outlining");

            await windowFrameService.SubscribeToWindowFrameEvents();

            return CodeDocumentViewModel;
        }
        catch (Exception e)
        {
            await LogHelper.LogException(this, "Error updating CodeDocumentViewModel", e);
        }

        return CodeDocumentViewModel;
    }

    public async Task LoadGlobalSettings(bool readFromDisk = false)
    {
        try
        {
            if (readFromDisk)
            {
                GlobalSettings = null;
            }

            GlobalSettings ??= await SettingsHelper.LoadGlobalSettings(this);

            SettingsDialogData = new()
            {
                AutoHighlight = GlobalSettings.AutoHighlight,
                AutoLoadLineThreshold = GlobalSettings.AutoLoadLineThreshold,
                ShowFilterToolbar = GlobalSettings.ShowFilterToolbar,
                ShowHistoryIndicators = GlobalSettings.ShowHistoryIndicators,
                UpdateWhileTyping = GlobalSettings.UpdateWhileTyping,
                EnableCrashAnalytics = GlobalSettings.EnableCrashAnalytics,
            };

            var filterRules = GlobalSettings
                .FilterRules
                .Select(filterRule => new FilterRuleViewModel
                {
                    Access = filterRule.Access,
                    Hide = filterRule.Hide,
                    Ignore = filterRule.Ignore,
                    IsEmpty = filterRule.IsEmpty,
                    Kind = filterRule.Kind,
                    Opacity = filterRule.Opacity,
                    Italic = filterRule.Italic,
                    FontScale = filterRule.FontScale,
                });

            FilterDialogData = new()
            {
                FilterRules = new ObservableList<FilterRuleViewModel>(filterRules) ?? [],
            };

            CodeDocumentViewModel.FilterRules = [.. FilterDialogData.FilterRules];

            //Update the filter toolbar visibility
            CodeDocumentViewModel.ShowFilterToolbarVisibility = SettingsDialogData.ShowFilterToolbar
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Clear any history indicators if the setting was turned off
            if (SettingsDialogData.ShowHistoryIndicators == false)
            {
                HistoryHelper.ClearHistory(CodeDocumentViewModel);
            }

            // Clear any highlights if the setting was turned off
            if (SettingsDialogData.AutoHighlight == false)
            {
                HighlightHelper.UnHighlight(CodeDocumentViewModel);
            }

            // Update the view model with the filter rules
            VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel, CodeDocumentViewModel.CodeItems, CodeDocumentViewModel.FilterRules);

            // Apply filter rules
            FilterRuleHelper.ApplyFilterRules(CodeDocumentViewModel, CodeDocumentViewModel.CodeItems, CodeDocumentViewModel.FilterRules);

            // Update the view model with the sort order
            SortHelper.ApplySort(CodeDocumentViewModel, GlobalSettings.SortOrder);
        }
        catch (Exception e)
        {
            await LogHelper.LogException(this, "Error loading global settings", e);
        }
    }

    private async Task HideToolWindow(CancellationToken cancellationToken)
    {
        if (ToolWindow == null)
        {
            return;
        }

        await ToolWindow.HideAsync(cancellationToken);
    }

    private async Task ShowToolWindow(CancellationToken cancellationToken)
    {
        if (ToolWindow == null)
        {
            return;
        }

        await ToolWindow.ShowAsync(activate: false, cancellationToken);
    }
}
