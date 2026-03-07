using CodeNav.Dialogs.FilterDialog;
using CodeNav.Dialogs.SettingsDialog;
using CodeNav.Helpers;
using CodeNav.Languages.CSharp.Mappers;
using CodeNav.Settings.Settings;
using CodeNav.ViewModels;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;

namespace CodeNav.Services;

#pragma warning disable VSEXTPREVIEW_SETTINGS // Type is for evaluation purposes only and is subject to change or removal in future updates.

public class CodeDocumentService
{
    public CodeDocumentService(CodeNavSettingsCategoryObserver settingsObserver)
    {
        settingsObserver.Changed += SettingsObserver_ChangedAsync;
    }

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

    public async Task<CodeDocumentViewModel> UpdateCodeDocumentViewModel(
        VisualStudioExtensibility? extensibility,
        ITextViewSnapshot? textView,
        CancellationToken cancellationToken)
    {
        if (extensibility == null ||
            textView == null)
        {
            return CodeDocumentViewModel;
        }

        // Show loading item while we process the document
        CodeDocumentViewModel.CodeItems = [.. PlaceholderHelper.CreateLoadingItem()];

        // Get the new list of code items
        var codeItems = await DocumentMapper.MapDocument(
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

    private Task SettingsObserver_ChangedAsync(CodeNavSettingsCategorySnapshot settingsSnapshot)
    {
        SettingsDialogData = SettingsHelper.GetSettings(settingsSnapshot);

        FilterDialogData = SettingsHelper.GetFilterRules(settingsSnapshot);

        CodeDocumentViewModel.FilterRules = [.. FilterDialogData.FilterRules];

        // Update the filter toolbar visibility
        CodeDocumentViewModel.ShowFilterToolbarVisibility =
            SettingsHelper.GetShowFilterToolbarVisibility(settingsSnapshot);

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
        var sortOrder = SettingsHelper.GetSortOrder(settingsSnapshot);
        SortHelper.ApplySort(CodeDocumentViewModel, sortOrder);

        return Task.CompletedTask;
    }
}
