using CodeNav.Dialogs.FilterDialog;
using CodeNav.Dialogs.SettingsDialog;
using CodeNav.Helpers;
using CodeNav.Languages.CSharp.Mappers;
using CodeNav.ViewModels;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Windows;

namespace CodeNav.Services;

public class CodeDocumentService(DocumentMapper documentMapper)
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

    public async Task<CodeDocumentViewModel> UpdateCodeDocumentViewModel(
        VisualStudioExtensibility? extensibility,
        ITextViewSnapshot? textView,
        CancellationToken cancellationToken)
    {
        try
        {
            if (extensibility == null ||
                textView == null)
            {
                return CodeDocumentViewModel;
            }

            // Show loading item while we process the document
            CodeDocumentViewModel.CodeItems = [.. PlaceholderHelper.CreateLoadingItem()];

            // Get the new list of code items
            var codeItems = await documentMapper.MapDocument(
                textView.Document,
                textView.FilePath,
                CodeDocumentViewModel,
                extensibility,
                cancellationToken);

            // Sort the list of code items,
            // And update the DataContext for the tool window
            CodeDocumentViewModel.CodeItems = SortHelper.Sort(codeItems, CodeDocumentViewModel.SortOrder);

            // Set properties on the CodeDocumentViewModel that are needed for other features
            CodeDocumentViewModel.CodeDocumentService = this;
            CodeDocumentViewModel.FilePath = textView.FilePath ?? string.Empty;

            // Apply highlights
            HighlightHelper.UnHighlight(CodeDocumentViewModel);

            // Apply current visibility settings to the document
            VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel.CodeItems, CodeDocumentViewModel.FilterRules);

            // Apply history items
            HistoryHelper.ApplyHistoryIndicator(CodeDocumentViewModel);

            // Apply bookmarks
            BookmarkHelper.ApplyBookmarkIndicator(CodeDocumentViewModel);

            // TODO: If we get support for OutliningManager in the future,
            // we should consider not expanding by default
            OutliningHelper.ExpandAll(CodeDocumentViewModel);

            return CodeDocumentViewModel;
        }
        catch (Exception e)
        {
            LogHelper.Log("Error updating CodeDocumentViewModel", e);
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

            GlobalSettings ??= await SettingsHelper.LoadGlobalSettings();

            SettingsDialogData = new()
            {
                AutoHighlight = GlobalSettings.AutoHighlight,
                AutoLoadLineThreshold = GlobalSettings.AutoLoadLineThreshold,
                ShowFilterToolbar = GlobalSettings.ShowFilterToolbar,
                ShowHistoryIndicators = GlobalSettings.ShowHistoryIndicators,
                UpdateWhileTyping = GlobalSettings.UpdateWhileTyping,
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
            VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel.CodeItems, CodeDocumentViewModel.FilterRules);

            // Update the view model with the sort order
            SortHelper.ApplySort(CodeDocumentViewModel, GlobalSettings.SortOrder);
        }
        catch (Exception e)
        {
            LogHelper.Log("Error loading global settings", e);
        }
    }
}
