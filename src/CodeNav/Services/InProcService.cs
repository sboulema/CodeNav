using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace CodeNav.Services;

[VisualStudioContribution]
internal class InProcService : IInProcService
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly MefInjection<IVsEditorAdaptersFactoryService> _editorAdaptersFactoryService;
    private readonly AsyncServiceProviderInjection<SVsTextManager, IVsTextManager> _textManager;

    public InProcService(
        VisualStudioExtensibility extensibility,
        MefInjection<IVsEditorAdaptersFactoryService> editorAdaptersFactoryService,
        AsyncServiceProviderInjection<SVsTextManager, IVsTextManager> textManager)
    {
        _extensibility = extensibility;
        _editorAdaptersFactoryService = editorAdaptersFactoryService;
        _textManager = textManager;
    }

    [VisualStudioContribution]
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(IInProcService.Configuration.ServiceName, IInProcService.Configuration.ServiceVersion, typeof(InProcService))
        {
            ServiceAudience = BrokeredServiceAudience.Local | BrokeredServiceAudience.Public,
        };

    public async Task DoSomethingAsync(CancellationToken cancellationToken)
    {
        await _extensibility.Shell().ShowPromptAsync("Hello from out-of-proc! (Showing this message from (in-proc)", PromptOptions.OK, cancellationToken);
    }

    public async Task TextViewScrollToSpan(int start, int length)
    {
        try
        {
            // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support viewscroller yet.
            var textView = await GetCurrentTextViewAsync();

            var span = new SnapshotSpan(textView.TextSnapshot, new Span(start, length));

            // Switch to the UI thread to ensure we can interact with the view scroller.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            textView.ViewScroller.EnsureSpanVisible(span, EnsureSpanVisibleOptions.AlwaysCenter);
        }
        catch (Exception)
        {
            // TODO: Implement in-proc error logging
        }
    }

    private async Task<IWpfTextView> GetCurrentTextViewAsync()
    {
        IVsEditorAdaptersFactoryService editorAdapter = await _editorAdaptersFactoryService.GetServiceAsync();
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
}
