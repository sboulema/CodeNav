using CodeNav.OutOfProc.Services;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;

namespace CodeNav.Services;

[VisualStudioContribution]
internal class InProcService : IInProcService
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly MefInjection<IVsEditorAdaptersFactoryService> _editorAdaptersFactoryService;
    private readonly MefInjection<IOutliningManagerService> _outliningManagerFactoryService;
    private readonly AsyncServiceProviderInjection<SVsTextManager, IVsTextManager> _textManager;

    public InProcService(
        VisualStudioExtensibility extensibility,
        MefInjection<IVsEditorAdaptersFactoryService> editorAdaptersFactoryService,
        MefInjection<IOutliningManagerService> outliningManagerFactoryService,
        AsyncServiceProviderInjection<SVsTextManager, IVsTextManager> textManager)
    {
        _extensibility = extensibility;
        _editorAdaptersFactoryService = editorAdaptersFactoryService;
        _outliningManagerFactoryService = outliningManagerFactoryService;
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

    public async Task SubscribeToRegionEvents()
    {
        // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support outlining yet.
        var textView = await GetCurrentTextViewAsync();

        var outliningManager = await GetOutliningManager(textView);

        outliningManager.RegionsExpanded -= OutliningManager_RegionsExpanded;
        outliningManager.RegionsExpanded += OutliningManager_RegionsExpanded;

        outliningManager.RegionsCollapsed -= OutliningManager_RegionsCollapsed;
        outliningManager.RegionsCollapsed += OutliningManager_RegionsCollapsed;
    }

    private async void OutliningManager_RegionsExpanded(object sender, RegionsExpandedEventArgs e)
    {
        var outOfProcService = await _extensibility.ServiceBroker
            .GetProxyAsync<IOutOfProcService>(IOutOfProcService.Configuration.ServiceDescriptor, cancellationToken: default);

        try
        {
            Assumes.NotNull(outOfProcService);

            foreach (var region in e.ExpandedRegions)
            {
                var span = GetSpan(region);
                await outOfProcService.SetCodeItemIsExpanded(span.Start, span.End, isExpanded: true);
            }
        }
        finally
        {
            (outOfProcService as IDisposable)?.Dispose();
        }
    }

    private async void OutliningManager_RegionsCollapsed(object sender, RegionsCollapsedEventArgs e)
    {
        var outOfProcService = await _extensibility.ServiceBroker
            .GetProxyAsync<IOutOfProcService>(IOutOfProcService.Configuration.ServiceDescriptor, cancellationToken: default);

        try
        {
            Assumes.NotNull(outOfProcService);

            foreach (var region in e.CollapsedRegions)
            {
                var span = GetSpan(region);
                await outOfProcService.SetCodeItemIsExpanded(span.Start, span.End, isExpanded: false);
            }
        }
        finally
        {
            (outOfProcService as IDisposable)?.Dispose();
        }
    }

    public async Task ExpandOutlineRegion(int start, int length)
    {
        try
        {
            // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support outlining yet.
            var textView = await GetCurrentTextViewAsync();

            var outliningManager = await GetOutliningManager(textView);

            // Switch to the UI thread to ensure we can interact with the outline regions.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var outlineRegion = await GetOutlineRegionForSpan(textView, outliningManager, start, length);

            // Check if the outline region is collapsed before expanding
            if (outlineRegion?.IsCollapsed != true ||
                outlineRegion is not ICollapsed collapsedOutlineRegion)
            {
                return;
            }

            outliningManager.Expand(collapsedOutlineRegion);
        }
        catch (Exception)
        {
            // TODO: Implement in-proc error logging
        }
    }

    public async Task CollapseOutlineRegion(int start, int length)
    {
        try
        {
            // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support outlining yet.
            var textView = await GetCurrentTextViewAsync();

            var outliningManager = await GetOutliningManager(textView);

            // Switch to the UI thread to ensure we can interact with the outline regions.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var outlineRegion = await GetOutlineRegionForSpan(textView, outliningManager, start, length);

            outliningManager.TryCollapse(outlineRegion);
        }
        catch (Exception)
        {
            // TODO: Implement in-proc error logging
        }
    }

    private async Task<ICollapsible?> GetOutlineRegionForSpan(
        IWpfTextView textView, IOutliningManager outliningManager,
        int start, int length)
    {
        // Get all outline regions for the given span
        var span = new SnapshotSpan(textView.TextSnapshot, start, length);

        var outlineRegions = outliningManager.GetAllRegions(span);

        // Get the first outline region that has the same span start or end
        return outlineRegions.FirstOrDefault(outlineRegion
            => GetSpan(outlineRegion).Start == start ||
               GetSpan(outlineRegion).End == start + length);
    }

    private SnapshotSpan GetSpan(ICollapsible outlineRegion)
        => outlineRegion.Extent.GetSpan(outlineRegion.Extent.TextBuffer.CurrentSnapshot);

    private async Task<IOutliningManager> GetOutliningManager(IWpfTextView textView)
    {
        var outliningManagerService = await _outliningManagerFactoryService.GetServiceAsync();
        var outliningManager = outliningManagerService.GetOutliningManager(textView);

        return outliningManager;
    }

    public async Task TextViewScrollToSpan(int start, int length)
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

    private async Task<IWpfTextView> GetCurrentTextViewAsync()
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
}
