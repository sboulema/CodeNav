using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Services;
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
/// <param name="diagnosticsProvider">Local diagnostics provider service instance.</param>
[VisualStudioContribution]
internal class TextViewEventListener(
    ExtensionEntrypoint extension,
    VisualStudioExtensibility extensibility,
    CodeDocumentService codeDocumentService)
    : ExtensionPart(extension, extensibility), ITextViewOpenClosedListener, ITextViewChangedListener
{
    private readonly CodeDocumentService codeDocumentService = Requires.NotNull(codeDocumentService, nameof(codeDocumentService));

    /// <inheritdoc/>
    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromDocumentType("CSharp"),
            DocumentFilter.FromGlobPattern("**/*.cs", true),
        ],
    };

    /// <inheritdoc />
    public async Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
    {
        try
        {
            await codeDocumentService.LoadGlobalSettings();

            // if the document is too large, skip processing to avoid performance issues
            if (args.AfterTextView.Document.Lines.Count >= codeDocumentService.SettingsDialogData.AutoLoadLineThreshold &&
                codeDocumentService.SettingsDialogData.AutoLoadLineThreshold > 0)
            {
                // Show the "line threshold passed" placeholder if the document exceeds the line threshold for auto-loading
                codeDocumentService.CodeDocumentViewModel.CodeItems = PlaceholderHelper.CreateLineThresholdPassedItem();

                return;
            }

            // Document changed:
            // - File path changed
            // - Edits made in the document
            // Action:
            // - Update code items list
            if ((args.Edits.Any() && codeDocumentService.SettingsDialogData.UpdateWhileTyping) ||
                args.AfterTextView.FilePath != codeDocumentService.CodeDocumentViewModel.FilePath)
            {
                await codeDocumentService.UpdateCodeDocumentViewModel(Extensibility, args.AfterTextView, cancellationToken);
            }

            // Document changed - Update history indicators
            if (args.Edits.Any() &&
                codeDocumentService.SettingsDialogData.ShowHistoryIndicators)
            {
                await HistoryHelper.AddItemToHistory(codeDocumentService.CodeDocumentViewModel, args.Edits);
            }

            // Selection changed - Update highlights
            if (args.BeforeTextView.Selection.ActivePosition.GetContainingLine().LineNumber !=
                args.AfterTextView.Selection.ActivePosition.GetContainingLine().LineNumber &&
                codeDocumentService.SettingsDialogData.AutoHighlight)
            {
                await HighlightHelper.HighlightCurrentItem(
                    codeDocumentService.CodeDocumentViewModel,
                    args.AfterTextView.Selection.ActivePosition.Offset);
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
            codeDocumentService.CodeDocumentViewModel.CodeItems = PlaceholderHelper.CreateSelectDocumentItem();
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

            if (textViewSnapshot.Document.Lines.Count >= codeDocumentService.SettingsDialogData.AutoLoadLineThreshold &&
                codeDocumentService.SettingsDialogData.AutoLoadLineThreshold > 0)
            {
                // Show the "line threshold passed" placeholder if the document exceeds the line threshold for auto-loading
                codeDocumentService.CodeDocumentViewModel.CodeItems = PlaceholderHelper.CreateLineThresholdPassedItem();

                return;
            }

            await codeDocumentService.UpdateCodeDocumentViewModel(Extensibility, textViewSnapshot, cancellationToken);
        }
        catch (Exception e)
        {
            await LogHelper.LogException(codeDocumentService, "Error listening to TextViewOpened", e);
        }
    }
}