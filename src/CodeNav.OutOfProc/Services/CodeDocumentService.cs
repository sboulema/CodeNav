using CodeNav.OutOfProc.Dialogs.FilterDialog;
using CodeNav.OutOfProc.Dialogs.SettingsDialog;
using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Models;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.Extensibility.UI;
using System.Windows;
using System.Windows.Controls;
using CSharpDocumentMapper = CodeNav.OutOfProc.Languages.CSharp.Mappers.DocumentMapper;
using TypeScriptDocumentMapper = CodeNav.OutOfProc.Languages.TypeScript.Mappers.DocumentMapper;
using VisualBasicDocumentMapper = CodeNav.OutOfProc.Languages.VisualBasic.Mappers.DocumentMapper;

namespace CodeNav.OutOfProc.Services;

public class CodeDocumentService
{
    private readonly OutputWindowService logService;
    private readonly OutliningService outliningService;
    private readonly WindowFrameService windowFrameService;

    private readonly List<IDocumentMapper> documentMappers =
    [
        new CSharpDocumentMapper(),
        new VisualBasicDocumentMapper(),
        new TypeScriptDocumentMapper(),
    ];

    // The last active document seen, used to immediately populate a newly opened tool
    // window (see issue #186) instead of leaving it empty until the next document change.
    private VisualStudioExtensibility? lastExtensibility;
    private string? lastFilePath;
    private string? lastText;

    public CodeDocumentService(
        OutputWindowService logService,
        OutliningService outliningService,
        WindowFrameService windowFrameService)
    {
        this.logService = logService;
        this.outliningService = outliningService;
        this.windowFrameService = windowFrameService;

        CodeDocumentViewModels.Add(new CodeDocumentViewModel
        {
            CodeDocumentService = this,
        });
    }

    /// <summary>
    /// DataContext for every open CodeNav tool window. There is always at least one entry,
    /// the primary tool window. A second, independent window (see issue #186) is added
    /// through <see cref="RegisterWindow"/> when it's opened via the duplicate toolbar button.
    /// </summary>
    public List<CodeDocumentViewModel> CodeDocumentViewModels { get; } = [];

    /// <summary>
    /// DataContext for the primary tool window.
    /// </summary>
    public CodeDocumentViewModel CodeDocumentViewModel => CodeDocumentViewModels[0];

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

    /// <summary>
    /// Registers the second CodeNav tool window (see issue #186), opened via the duplicate
    /// toolbar button, and immediately populates it with the last known active document.
    /// </summary>
    /// <param name="toolWindow">The newly opened tool window</param>
    /// <returns>A new, independent view model for the tool window</returns>
    public CodeDocumentViewModel RegisterWindow(ToolWindow toolWindow)
    {
        var viewModel = new CodeDocumentViewModel
        {
            CodeDocumentService = this,
            ToolWindow = toolWindow,
        };

        CodeDocumentViewModels.Add(viewModel);

        return viewModel;
    }

    /// <summary>
    /// Populate a newly registered tool window with the last known active document,
    /// so it isn't empty until the next document change.
    /// </summary>
    public async Task RefreshWindow(CodeDocumentViewModel viewModel, CancellationToken cancellationToken)
    {
        if (lastExtensibility == null || lastFilePath == null || lastText == null)
        {
            return;
        }

        await UpdateCodeDocumentViewModel(lastExtensibility, lastFilePath, lastText, viewModel, cancellationToken);
    }

    /// <summary>
    /// Update every non-pinned ViewModel with the given, newly active document.
    /// </summary>
    public async Task UpdateCodeDocumentViewModels(
        VisualStudioExtensibility? extensibility,
        string? filePath,
        string? text,
        CancellationToken cancellationToken)
    {
        lastFilePath = filePath;
        lastText = text;

        var activeCodeDocumentViewModels = CodeDocumentViewModels
            .Where(window => !(window.IsPinned && window.FilePath != filePath))
            .ToList();

        foreach (var codeDocumentViewModel in activeCodeDocumentViewModels)
        {
            await UpdateCodeDocumentViewModel(extensibility, filePath, text, codeDocumentViewModel, cancellationToken);
        }
    }

    /// <summary>
    /// Update the view model
    /// </summary>
    /// <param name="extensibility">Visual Studio extensibility</param>
    /// <param name="filePath">The path to the file</param>
    /// <param name="text">The text of the file</param>
    /// <param name="codeDocumentViewModel">The view model of the tool window to update</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated code document view model</returns>
    public async Task<CodeDocumentViewModel> UpdateCodeDocumentViewModel(
        VisualStudioExtensibility? extensibility,
        string? filePath,
        string? text,
        CodeDocumentViewModel codeDocumentViewModel,
        CancellationToken cancellationToken)
    {
        try
        {
            if (extensibility != null)
            {
                lastExtensibility = extensibility;
            }

            if (extensibility == null ||
                string.IsNullOrEmpty(filePath) ||
                string.IsNullOrEmpty(text))
            {
                return codeDocumentViewModel;
            }

            await LoadGlobalSettings();

            var documentMapper = documentMappers
                .FirstOrDefault(mapper =>
                    mapper.CanMapDocument(filePath, GlobalSettings!));

            if (documentMapper == null)
            {
                codeDocumentViewModel.CodeItems =
                    PlaceholderHelper.CreateNoCodeItemsFound();

                // No code items found, hide the tool window after showing the "No code items found" message
                await HideToolWindow(codeDocumentViewModel, cancellationToken);

                return codeDocumentViewModel;
            }

            // Show loading item while we process the document
            var loadingCancellationTokenSource = new CancellationTokenSource();
            var loadingCancellationToken = loadingCancellationTokenSource.Token;

            _ = PlaceholderHelper.CreateLoadingItem(
                codeDocumentViewModel,
                500,
                loadingCancellationToken);

            // Get the new list of code items
            var codeItems = await documentMapper.MapDocument(
                text,
                filePath,
                codeDocumentViewModel,
                extensibility,
                cancellationToken);

            await logService.WriteInfo(filePath, $"Found '{codeItems.Count}' code items");

            // Getting the new code items is done, cancel creating a loading placeholder
            await loadingCancellationTokenSource.CancelAsync();

            // Set properties on the CodeDocumentViewModel that are needed for other features
            codeDocumentViewModel.FilePath = filePath ?? string.Empty;

            if (!codeItems.Any())
            {
                codeDocumentViewModel.CodeItems = PlaceholderHelper.CreateNoCodeItemsFound();

                // No code items found, hide the tool window after showing the "No code items found" message
                await HideToolWindow(codeDocumentViewModel, cancellationToken);
                
                return codeDocumentViewModel;
            }

            // Code items were found, make sure the tool window is visible
            await ShowToolWindow(codeDocumentViewModel, cancellationToken);

            // Sort the list of code items,
            // And update the DataContext for the tool window
            codeDocumentViewModel.CodeItems = SortHelper.Sort(codeItems, codeDocumentViewModel.SortOrder);

            await logService.WriteInfo(filePath, $"Sorted code items on '{codeDocumentViewModel.SortOrder}'");

            // Apply highlights
            HighlightHelper.UnHighlight(codeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Remove highlight from all code items");

            // Apply current visibility settings to the document
            VisibilityHelper.SetCodeItemVisibility(
                codeDocumentViewModel,
                codeDocumentViewModel.CodeItems,
                codeDocumentViewModel.FilterRules,
                codeDocumentViewModel.FilterText,
                codeDocumentViewModel.BookmarkIds);

            await logService.WriteInfo(filePath, $"Set code item visibility");

            // Apply filter rules
            FilterRuleHelper.ApplyFilterRules(codeDocumentViewModel, codeDocumentViewModel.CodeItems, codeDocumentViewModel.FilterRules);

            await logService.WriteInfo(filePath, $"Set code item filter rules");

            // Apply history items
            HistoryHelper.ApplyHistoryIndicator(codeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Apply history indicators");

            // Apply bookmarks
            BookmarkHelper.ApplyBookmarkIndicator(codeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Apply bookmark indicators");

            // Apply outlining
            await OutliningService.SubscribeToRegionEvents(codeDocumentViewModel);

            await logService.WriteInfo(filePath, $"Apply outlining");

            await windowFrameService.SubscribeToWindowFrameEvents();

            return codeDocumentViewModel;
        }
        catch (Exception e)
        {
            await LogHelper.LogException(this, "Error updating CodeDocumentViewModel", e);
        }

        return codeDocumentViewModel;
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
                ShowToolWindowForUnsupportedFiles = GlobalSettings.ShowToolWindowForUnsupportedFiles,
                UseCompactMode = GlobalSettings.UseCompactMode,
                EnableCSharp = GlobalSettings.EnableCSharp,
                EnableVisualBasic = GlobalSettings.EnableVisualBasic,
                EnableTypeScript = GlobalSettings.EnableTypeScript,
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

            // Apply settings to every open CodeNav tool window (see issue #186)
            foreach (var window in CodeDocumentViewModels)
            {
                window.FilterRules = [.. FilterDialogData.FilterRules];

                // Update the filter toolbar visibility
                window.ShowFilterToolbarVisibility = SettingsDialogData.ShowFilterToolbar
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                // Update the usage of compact mode
                window.UseCompactMode = SettingsDialogData.UseCompactMode;

                // Clear any history indicators if the setting was turned off
                if (SettingsDialogData.ShowHistoryIndicators == false)
                {
                    HistoryHelper.ClearHistory(window);
                }

                // Clear any highlights if the setting was turned off
                if (SettingsDialogData.AutoHighlight == false)
                {
                    HighlightHelper.UnHighlight(window);
                }

                // Update the view model with the filter rules
                VisibilityHelper.SetCodeItemVisibility(window, window.CodeItems, window.FilterRules);

                // Apply filter rules
                FilterRuleHelper.ApplyFilterRules(window, window.CodeItems, window.FilterRules);

                // Update the view model with the sort order
                SortHelper.ApplySort(window, GlobalSettings.SortOrder);
            }
        }
        catch (Exception e)
        {
            await LogHelper.LogException(this, "Error loading global settings", e);
        }
    }

    public async Task HideToolWindow(CodeDocumentViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.ToolWindow == null)
        {
            return;
        }

        if (GlobalSettings!.ShowToolWindowForUnsupportedFiles)
        {
            return;
        }

        await viewModel.ToolWindow.HideAsync(cancellationToken);
    }

    private async Task ShowToolWindow(CodeDocumentViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.ToolWindow == null)
        {
            return;
        }

        if (GlobalSettings!.ShowToolWindowForUnsupportedFiles)
        {
            return;
        }

        await viewModel.ToolWindow.ShowAsync(activate: false, cancellationToken);
    }
}
