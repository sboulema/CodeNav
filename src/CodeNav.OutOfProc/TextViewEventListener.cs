using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Services;
using DebounceThrottle;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;

namespace CodeNav.OutOfProc;

/// <summary>
/// Listener for text view lifetime events to start CodeNav on new documents or changed documents.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TextViewEventListener"/> class.
/// </remarks>
/// <param name="extension">Extension instance.</param>
/// <param name="extensibility">Extensibility object.</param>
/// <param name="codeDocumentService">CodeDocumentService object.</param>
[VisualStudioContribution]
internal class TextViewEventListener(
    ExtensionEntrypoint extension,
    VisualStudioExtensibility extensibility,
    CodeDocumentService codeDocumentService)
    : ExtensionPart(extension, extensibility), ITextViewOpenClosedListener, ITextViewChangedListener
{
    private readonly CodeDocumentService codeDocumentService = Requires.NotNull(codeDocumentService, nameof(codeDocumentService));

    private readonly DebounceDispatcher debounceDispatcher = new(TimeSpan.FromMilliseconds(300));

    /// <inheritdoc/>
    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromGlobPattern("**/*.{cs,vb,ts,tsx}", true),
        ],
    };

    /// <inheritdoc />
    public async Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
    {
        try
        {
            await codeDocumentService.LoadGlobalSettings();

            // Windows pinned to a different document (see issue #186) don't follow the active document
            var activeCodeDocumentViewModels = codeDocumentService.CodeDocumentViewModels
                .Where(model => !(model.IsPinned && model.FilePath != args.AfterTextView.FilePath))
                .ToList();

            // if the document is too large, skip processing to avoid performance issues
            if (args.AfterTextView.Document.Lines.Count >= codeDocumentService.SettingsDialogData.AutoLoadLineThreshold &&
                codeDocumentService.SettingsDialogData.AutoLoadLineThreshold > 0)
            {
                // Show the "line threshold passed" placeholder if the document exceeds the line threshold for auto-loading
                foreach (var window in activeCodeDocumentViewModels)
                {
                    window.CodeItems = PlaceholderHelper.CreateLineThresholdPassedItem();
                }

                return;
            }

            // Document changed - Update code items list
            if ((args.Edits.Any() &&
                codeDocumentService.SettingsDialogData.UpdateWhileTyping))
            {
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                await debounceDispatcher.DebounceAsync(async () =>
                {
                    await codeDocumentService.UpdateCodeDocumentViewModels(
                        Extensibility,
                        args.AfterTextView.FilePath,
                        args.AfterTextView.Document.Text.CopyToString(),
                        cancellationToken);
                },
                cancellationToken);
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
            }

            // Document changed - Update history indicators
            if (args.Edits.Any() &&
                codeDocumentService.SettingsDialogData.ShowHistoryIndicators)
            {
                foreach (var window in activeCodeDocumentViewModels)
                {
                    await HistoryHelper.AddItemToHistory(window, args.Edits);
                }
            }

            // Selection changed - Update highlights
            if (args.BeforeTextView.Selection.ActivePosition.GetContainingLine().LineNumber !=
                args.AfterTextView.Selection.ActivePosition.GetContainingLine().LineNumber &&
                codeDocumentService.SettingsDialogData.AutoHighlight)
            {
                foreach (var window in activeCodeDocumentViewModels)
                {
                    await HighlightHelper.HighlightCurrentItem(
                        window,
                        args.AfterTextView.Selection.ActivePosition.Offset);
                }
            }
        }
        catch (Exception e)
        {
            await LogHelper.LogException(codeDocumentService, "Error listening to TextViewChanged", e);
        }
    }

    /// <inheritdoc />
    public async Task TextViewClosedAsync(ITextViewSnapshot textViewSnapshot, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var window in codeDocumentService.CodeDocumentViewModels)
            {
                // If this window is pinned to a different document, leave it untouched
                if (window.IsPinned && textViewSnapshot.FilePath != window.FilePath)
                {
                    continue;
                }

                // The pinned document itself was closed, unpin so this window can follow the active document again
                window.IsPinned = false;

                window.CodeItems = PlaceholderHelper.CreateSelectDocumentItem();

                await codeDocumentService.HideToolWindow(window, cancellationToken);
            }
        }
        catch (Exception e)
        {
            await LogHelper.LogException(codeDocumentService, "Error listening to TextViewClosed", e);
        }
    }

    /// <inheritdoc />
    public async Task TextViewOpenedAsync(ITextViewSnapshot textViewSnapshot, CancellationToken cancellationToken)
    {
        try
        {
            await codeDocumentService.LoadGlobalSettings();

            // Windows pinned to a different document (see issue #186) don't follow the active document
            var activeWindows = codeDocumentService.CodeDocumentViewModels
                .Where(window => !(window.IsPinned && window.FilePath != textViewSnapshot.FilePath))
                .ToList();

            if (textViewSnapshot.Document.Lines.Count >= codeDocumentService.SettingsDialogData.AutoLoadLineThreshold &&
                codeDocumentService.SettingsDialogData.AutoLoadLineThreshold > 0)
            {
                // Show the "line threshold passed" placeholder if the document exceeds the line threshold for auto-loading
                foreach (var window in activeWindows)
                {
                    window.CodeItems = PlaceholderHelper.CreateLineThresholdPassedItem();
                }

                return;
            }

            await codeDocumentService.UpdateCodeDocumentViewModels(
                Extensibility,
                textViewSnapshot.FilePath,
                textViewSnapshot.Document.Text.CopyToString(),
                cancellationToken);
        }
        catch (Exception e)
        {
            await LogHelper.LogException(codeDocumentService, "Error listening to TextViewOpened", e);
        }
    }
}
