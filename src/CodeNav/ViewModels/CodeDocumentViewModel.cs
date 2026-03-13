using CodeNav.Constants;
using CodeNav.Dialogs.FilterDialog;
using CodeNav.Dialogs.SettingsDialog;
using CodeNav.Helpers;
using CodeNav.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.RpcContracts.Notifications;
using System.Runtime.Serialization;
using System.Windows;

namespace CodeNav.ViewModels;

[DataContract]
public class CodeDocumentViewModel : NotifyPropertyChangedObject
{
    public CodeDocumentViewModel()
    {
        // Main Toolbar
        RefreshCommand = new(Refresh);
        SortByFileCommand = new(SortByFile);
        SortByNameCommand = new(SortByName);
        ExpandAllCommand = new(ExpandAll);
        CollapseAllCommand = new(CollapseAll);
        SettingsCommand = new(Settings);

        // Filter Toolbar
        ClearFilterTextCommand = new(ClearFilterText);
        FilterCommand = new(Filter);
        FilterOnBookmarksCommand = new(FilterOnBookmarks);
    }

    public CodeDocumentService? CodeDocumentService { get; set; }

    public SortOrderEnum SortOrder = SortOrderEnum.SortByFile;

    public List<string> HistoryItemIds = [];

    public List<string> BookmarkIds = [];

    public List<FilterRuleViewModel> FilterRules = [];

    public string FilePath { get; set; } = string.Empty;

    #region Dependency properties

    private List<CodeItem> _codeItems = [];

    [DataMember]
    public List<CodeItem> CodeItems
    {
        get => _codeItems;
        set
        {
            _codeItems = value;
            // Always raise the PropertyChanged event,
            // different sortings of the list do not trigger
            // the default notification.
            RaiseNotifyPropertyChangedEvent();
        }
    }

    private Visibility _showFilterToolbarVisibility = Visibility.Visible;

    /// <summary>
    /// Visibility of the filter toolbar.
    /// </summary>
    [DataMember]
    public Visibility ShowFilterToolbarVisibility
    {
        get => _showFilterToolbarVisibility;
        set => SetProperty(ref _showFilterToolbarVisibility, value);
    }

    private string _filterText = string.Empty;

    /// <summary>
    /// Text to filter code items by name.
    /// </summary>
    [DataMember]
    public string FilterText
    {
        get => _filterText;
        set
        {
            SetProperty(ref _filterText, value);
            VisibilityHelper.SetCodeItemVisibility(CodeItems, FilterRules, value);
            RaiseNotifyPropertyChangedEvent(nameof(ShowClearFilterTextButtonVisibility));
        }
    }

    /// <summary>
    /// Visibility of the clear filter text toolbar button.
    /// </summary>
    [DataMember]
    public Visibility ShowClearFilterTextButtonVisibility
        => string.IsNullOrEmpty(FilterText)
            ? Visibility.Collapsed
            : Visibility.Visible;

    private Visibility _showFilterOnBookmarksButtonVisibility = Visibility.Collapsed;

    /// <summary>
    /// Visibility of the filter by bookmarks toolbar button.
    /// </summary>
    [DataMember]
    public Visibility ShowFilterOnBookmarksButtonVisibility
    {
        get => _showFilterOnBookmarksButtonVisibility;
        set => SetProperty(ref _showFilterOnBookmarksButtonVisibility, value);
    }

    private bool _isSortByNameChecked;

    /// <summary>
    /// Indicator if the main toolbar radio button should be checked.
    /// </summary>
    [DataMember]
    public bool IsSortByNameChecked
    {
        get => _isSortByNameChecked;
        set => SetProperty(ref _isSortByNameChecked, value);
    }

    private bool _isSortByFileChecked = true;

    /// <summary>
    /// Indicator if the main toolbar radio button should be checked.
    /// </summary>
    [DataMember]
    public bool IsSortByFileChecked
    {
        get => _isSortByFileChecked;
        set => SetProperty(ref _isSortByFileChecked, value);
    }

    #endregion

    #region Commands

    [DataMember]
    public AsyncCommand RefreshCommand { get; }
    private async Task Refresh(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        var textViewSnapshot = await clientContext.GetActiveTextViewAsync(cancellationToken);

        if (textViewSnapshot == null)
        {
            return;
        }

        if (CodeDocumentService == null)
        {
            return;
        }

        await CodeDocumentService.UpdateCodeDocumentViewModel(clientContext.Extensibility, textViewSnapshot, cancellationToken);
    }

    [DataMember]
    public AsyncCommand SortByNameCommand { get; }
    private async Task SortByName(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        var textViewSnapshot = await clientContext.GetActiveTextViewAsync(cancellationToken);

        if (textViewSnapshot == null)
        {
            return;
        }

        if (CodeDocumentService == null)
        {
            return;
        }

        await CodeDocumentService.LoadGlobalSettings();
        CodeDocumentService.GlobalSettings!.SortOrder = SortOrderEnum.SortByName;
        await SettingsHelper.SaveGlobalSettings(CodeDocumentService.GlobalSettings);

        SortHelper.ApplySort(CodeDocumentService.CodeDocumentViewModel, SortOrderEnum.SortByName);

        await CodeDocumentService.UpdateCodeDocumentViewModel(clientContext.Extensibility, textViewSnapshot, cancellationToken);
    }

    [DataMember]
    public AsyncCommand SortByFileCommand { get; }
    private async Task SortByFile(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        var textViewSnapshot = await clientContext.GetActiveTextViewAsync(cancellationToken);

        if (textViewSnapshot == null)
        {
            return;
        }

        if (CodeDocumentService == null)
        {
            return;
        }

        await CodeDocumentService.LoadGlobalSettings();
        CodeDocumentService.GlobalSettings!.SortOrder = SortOrderEnum.SortByFile;
        await SettingsHelper.SaveGlobalSettings(CodeDocumentService.GlobalSettings);

        SortHelper.ApplySort(CodeDocumentService.CodeDocumentViewModel, SortOrderEnum.SortByFile);

        await CodeDocumentService.UpdateCodeDocumentViewModel(clientContext.Extensibility, textViewSnapshot, cancellationToken);
    }

    [DataMember]
    public AsyncCommand ExpandAllCommand { get; }
    private async Task ExpandAll(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        OutliningHelper.ExpandAll(CodeDocumentService?.CodeDocumentViewModel);
    }

    [DataMember]
    public AsyncCommand CollapseAllCommand { get; }
    private async Task CollapseAll(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        OutliningHelper.CollapseAll(CodeDocumentService?.CodeDocumentViewModel);
    }

    [DataMember]
    public AsyncCommand SettingsCommand { get; }
    private async Task Settings(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        if (CodeDocumentService == null)
        {
            return;
        }

        await CodeDocumentService.LoadGlobalSettings(readFromDisk: true);

        var dialogResult = await clientContext.Extensibility.Shell().ShowDialogAsync(
            content: new SettingsDialogControl(CodeDocumentService.SettingsDialogData),
            title: "CodeNav Settings",
            options: new(DialogButton.OKCancel, DialogResult.OK),
            cancellationToken);

        if (dialogResult == DialogResult.Cancel)
        {
            return;
        }

        CodeDocumentService.GlobalSettings!.AutoHighlight = CodeDocumentService.SettingsDialogData.AutoHighlight;
        CodeDocumentService.GlobalSettings.AutoLoadLineThreshold = CodeDocumentService.SettingsDialogData.AutoLoadLineThreshold;
        CodeDocumentService.GlobalSettings.ShowFilterToolbar = CodeDocumentService.SettingsDialogData.ShowFilterToolbar;
        CodeDocumentService.GlobalSettings.ShowHistoryIndicators = CodeDocumentService.SettingsDialogData.ShowHistoryIndicators;
        CodeDocumentService.GlobalSettings.UpdateWhileTyping = CodeDocumentService.SettingsDialogData.UpdateWhileTyping;

        await SettingsHelper.SaveGlobalSettings(CodeDocumentService.GlobalSettings);

        // Refresh the tool window with the new settings
        await CodeDocumentService.LoadGlobalSettings(readFromDisk: true);
    }

    [DataMember]
    public AsyncCommand ClearFilterTextCommand { get; }
    private async Task ClearFilterText(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        FilterText = string.Empty;
    }

    [DataMember]
    public AsyncCommand FilterCommand { get; }
    private async Task Filter(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        if (CodeDocumentService == null)
        {
            return;
        }

        // Retrieve filter rules from settings
        await CodeDocumentService.LoadGlobalSettings(readFromDisk: true);

        // Open filter rule dialog
        var dialogResult = await clientContext.Extensibility.Shell().ShowDialogAsync(
            content: new FilterDialogControl(CodeDocumentService.FilterDialogData),
            title: "CodeNav Filters",
            options: new(DialogButton.OKCancel, DialogResult.OK),
            cancellationToken);

        if (dialogResult == DialogResult.Cancel)
        {
            return;
        }

        // Save new filter rules to settings
        CodeDocumentService.GlobalSettings!.FilterRules = [.. CodeDocumentService
            .FilterDialogData
            .FilterRules
            .Select(filterRule => new FilterRule
            {
                Access = filterRule.Access,
                Hide = filterRule.Hide,
                Ignore = filterRule.Ignore,
                IsEmpty = filterRule.IsEmpty,
                Kind = filterRule.Kind,
                Opacity = filterRule.Opacity,
            })];

        await SettingsHelper.SaveGlobalSettings(CodeDocumentService.GlobalSettings);

        // Save new filter rules to view model
        FilterRules = [.. CodeDocumentService.FilterDialogData.FilterRules];

        // Refresh the list of code items with the new filter rules
        await CodeDocumentService.LoadGlobalSettings(readFromDisk: true);
    }

    [DataMember]
    public AsyncCommand FilterOnBookmarksCommand { get; }
    private async Task FilterOnBookmarks(object? commandParameter, IClientContext clientContext, CancellationToken cancellationToken)
    {
        VisibilityHelper.SetCodeItemVisibility(CodeItems, FilterRules, FilterText, BookmarkIds);
    }

    #endregion
}
