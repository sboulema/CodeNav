using CodeNav.Models;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace CodeNav.Services;

public class TextViewService
{
    private readonly MefInjection<IVsEditorAdaptersFactoryService> _editorAdaptersFactoryService;
    private readonly AsyncServiceProviderInjection<SVsTextManager, IVsTextManager> _textManager;

#pragma warning disable IDE0290 // Use primary constructor
    public TextViewService(
#pragma warning restore IDE0290 // Use primary constructor
        MefInjection<IVsEditorAdaptersFactoryService> editorAdaptersFactoryService,
        AsyncServiceProviderInjection<SVsTextManager, IVsTextManager> textManager)
    {
        _editorAdaptersFactoryService = editorAdaptersFactoryService;
        _textManager = textManager;
    }

    /// <summary>
    /// Scroll to a given span in the text view
    /// </summary>
    /// <remarks>Caret will remain at its original position</remarks>
    /// <param name="start">Start position of the span</param>
    /// <param name="length">Length of the span</param>
    /// <returns></returns>
    public async Task ScrollToSpan(int start, int length)
    {
        try
        {
            // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support viewscroller yet.
            var textView = await GetCurrentTextViewAsync();

            if (!textView.TextBuffer.ContentType.TypeName.Equals("CSharp"))
            {
                // TODO: Log that the cursor and thus active text view is not in a csharp editor, for example the output window.
                return;
            }

            var span = new SnapshotSpan(textView.TextSnapshot, start, length);

            // Switch to the UI thread to ensure we can interact with the view scroller.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            textView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);
        }
        catch (Exception)
        {
            // TODO: Implement in-proc error logging
        }
    }

    /// <summary>
    /// Move the caret in the text view to the given position and keep the keyboard focus on the text view
    /// </summary>
    /// <param name="position">Position in the text view</param>
    /// <returns>Awaitable Task</returns>
    public async Task MoveCaretToPosition(int position)
    {
        try
        {
            // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support viewscroller yet.
            var textView = await GetCurrentTextViewAsync();

            if (!textView.TextBuffer.ContentType.TypeName.Equals("CSharp"))
            {
                // TODO: Log that the cursor and thus active text view is not in a csharp editor, for example the output window.
                return;
            }

            var snapShotPoint = new SnapshotPoint(textView.TextSnapshot, position);

            // Switch to the UI thread to ensure we can interact with the view scroller.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            textView.Caret.MoveTo(snapShotPoint);
            textView.VisualElement.Focus();
        }
        catch (Exception)
        {
            // TODO: Implement in-proc error logging
        }
    }

    public async Task<IWpfTextView> GetCurrentTextViewAsync()
    {
        var editorAdapter = await _editorAdaptersFactoryService.GetServiceAsync();
        var view = editorAdapter.GetWpfTextView(await GetCurrentNativeTextViewAsync());
        Assumes.Present(view);
        return view;
    }

    private async Task<IVsTextView> GetCurrentNativeTextViewAsync()
    {
        var textManager = await _textManager.GetServiceAsync();
        ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));
        return activeView;
    }

    /// <summary>
    /// Get the document view from the window frame
    /// </summary>
    /// <remarks>Document view is a combination of the file path and the text of a text document for a given frame</remarks>
    /// <param name="windowFrame"></param>
    /// <returns></returns>
    public async Task<DocumentView?> GetDocumentView(IVsWindowFrame windowFrame)
    {
        var textView = await GetTextView(windowFrame);

        if (textView == null)
        {
            return null;
        }

        var textDocument = GetTextDocument(textView);

        if (textDocument == null)
        {
            return null;
        }

        return new()
        {
            FilePath = textDocument?.FilePath ?? string.Empty,
            Text = textView.TextBuffer.CurrentSnapshot.GetText(),
            IsDocumentFrame = true,
        };
    }

    /// <summary>
    /// Gets the text view from the window frame.
    /// </summary>
    /// <returns><see langword="null"/> if the window isn't a document window.</returns>
    private async Task<IWpfTextView?> GetTextView(IVsWindowFrame? windowFrame)
    {
        if (windowFrame == null)
        {
            return null;
        }

        try
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Force the loading of a document that may be pending initialization.
            // See https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/delayed-document-loading
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out _);

            var nativeView = VsShellUtilities.GetTextView(windowFrame);

            if (nativeView == null)
            {
                return null;
            }

            var editorAdapter = await _editorAdaptersFactoryService.GetServiceAsync();

            var view = editorAdapter.GetWpfTextView(nativeView);

            return view;
        }
        catch (Exception)
        {
            // TODO: Implement in-proc error logging
        }

        return null;
    }

    /// <summary>
    /// Get the text document associated with the given text view
    /// </summary>
    /// <param name="textView">WPF Text View</param>
    /// <returns></returns>
    private static ITextDocument? GetTextDocument(IWpfTextView textView)
        => textView != null && textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document)
            ? document
            : null;
}
