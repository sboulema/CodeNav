using CodeNav.Models;
using CodeNav.OutOfProc.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using System.Text.Json;

namespace CodeNav.Services;

[VisualStudioContribution]
internal class InProcService : IInProcService, IVsWindowFrameEvents
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly TextViewService _textViewService;
    private readonly MefInjection<IOutliningManagerService> _outliningManagerFactoryService;
    private readonly AsyncServiceProviderInjection<SVsUIShell, IVsUIShell7> _vsUIShell;

    public InProcService(
        VisualStudioExtensibility extensibility,
        TextViewService textViewService,
        MefInjection<IOutliningManagerService> outliningManagerFactoryService,
        AsyncServiceProviderInjection<SVsUIShell, IVsUIShell7> vsUIShell)
    {
        _extensibility = extensibility;
        _textViewService = textViewService;
        _outliningManagerFactoryService = outliningManagerFactoryService;
        _vsUIShell = vsUIShell;
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

    #region Outlining

    /// <summary>
    /// Subscribe to outline events, so we will be notified if an outline region is changed.
    /// Get the current state of all regions
    /// </summary>
    /// <remarks>Used by the outproc service when mapping a document</remarks>
    /// <returns>JSON string with list of regions</returns>
    public async Task<string> SubscribeToRegionEvents()
    {
        // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support outlining yet.
        var textView = await _textViewService.GetCurrentTextViewAsync();

        var outliningManager = await GetOutliningManager(textView);

        // Subscribing to outline events
        outliningManager.RegionsExpanded -= OutliningManager_RegionsExpanded;
        outliningManager.RegionsExpanded += OutliningManager_RegionsExpanded;

        outliningManager.RegionsCollapsed -= OutliningManager_RegionsCollapsed;
        outliningManager.RegionsCollapsed += OutliningManager_RegionsCollapsed;

        var textViewSpan = new SnapshotSpan(textView.TextSnapshot, 0, textView.TextSnapshot.Length);

        // Switch to the UI thread to ensure we can interact with the outline regions.
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        // Get state of all current outline regions
        var outlineRegions = outliningManager.GetAllRegions(textViewSpan);

        var results = outlineRegions
            .Select(outlineRegion => new OutlineRegion
            {
                SpanStart = GetSpan(outlineRegion).Span.Start,
                SpanEnd = GetSpan(outlineRegion).Span.End,
                IsExpanded = !outlineRegion.IsCollapsed,
            });

        return JsonSerializer.Serialize(results);
    }

    /// <summary>
    /// Event handler triggered when an outline region is expanded
    /// </summary>
    /// <remarks>Used to set the matching code item to expanded</remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void OutliningManager_RegionsExpanded(object sender, RegionsExpandedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
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

    /// <summary>
    /// Event handler triggered when an outline region is collapsed
    /// </summary>
    /// <remarks>Used to set the matching code item to collapsed</remarks>
    /// <param name="sender"></param>
    /// <param name="e"></param>
#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void OutliningManager_RegionsCollapsed(object sender, RegionsCollapsedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
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

    /// <summary>
    /// Expand the outline region which has a matching span start and span end
    /// </summary>
    /// <remarks>Used when a code item is being expanded</remarks>
    /// <param name="spanStart">Start of the span being expanded</param>
    /// <param name="spanLength">Length of the span being expanded</param>
    /// <returns>Awaitable Task</returns>
    public async Task ExpandOutlineRegion(int spanStart, int spanLength)
    {
        try
        {
            // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support outlining yet.
            var textView = await _textViewService.GetCurrentTextViewAsync();

            var outliningManager = await GetOutliningManager(textView);

            // Switch to the UI thread to ensure we can interact with the outline regions.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var outlineRegion = await GetOutlineRegionForSpan(textView, outliningManager, spanStart, spanLength);

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

    /// <summary>
    /// Collapse the outline region which has a matching span start and span end
    /// </summary>
    /// <remarks>Used when a code item is being collapsed</remarks>
    /// <param name="spanStart">Start of the span being collapsed</param>
    /// <param name="spanLength">Length of the span being collapsed</param>
    /// <returns>Awaitable Task</returns>
    public async Task CollapseOutlineRegion(int spanStart, int spanLength)
    {
        try
        {
            // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support outlining yet.
            var textView = await _textViewService.GetCurrentTextViewAsync();

            var outliningManager = await GetOutliningManager(textView);

            // Switch to the UI thread to ensure we can interact with the outline regions.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var outlineRegion = await GetOutlineRegionForSpan(textView, outliningManager, spanStart, spanLength);

            outliningManager.TryCollapse(outlineRegion);
        }
        catch (Exception)
        {
            // TODO: Implement in-proc error logging
        }
    }

    /// <summary>
    /// Get the outline region for a given span
    /// </summary>
    /// <param name="textView">Current IWpfTextView</param>
    /// <param name="outliningManager">Outliningmanager</param>
    /// <param name="start">Start of the span</param>
    /// <param name="length">Length of the span</param>
    /// <returns></returns>
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

    /// <summary>
    /// Get the span for a given outline region
    /// </summary>
    /// <param name="outlineRegion">Outline region</param>
    /// <returns>SnapshotSpan for the region</returns>
    private SnapshotSpan GetSpan(ICollapsible outlineRegion)
        => outlineRegion.Extent.GetSpan(outlineRegion.Extent.TextBuffer.CurrentSnapshot);

    private async Task<IOutliningManager> GetOutliningManager(IWpfTextView textView)
    {
        var outliningManagerService = await _outliningManagerFactoryService.GetServiceAsync();
        var outliningManager = outliningManagerService.GetOutliningManager(textView);

        return outliningManager;
    }

    #endregion

    #region Text View

    /// <summary>
    /// Scroll to a given span in the text view
    /// </summary>
    /// <remarks>Caret will remain at its original position</remarks>
    /// <param name="start">Start position of the span</param>
    /// <param name="length">Length of the span</param>
    /// <returns></returns>
    public async Task TextViewScrollToSpan(int start, int length)
        => await _textViewService.ScrollToSpan(start, length);

    /// <summary>
    /// Move the caret in the text view to the given position and keep the keyboard focus on the text view
    /// </summary>
    /// <param name="position">Position in the text view</param>
    /// <returns>Awaitable Task</returns>
    public async Task TextViewMoveCaretToPosition(int position)
        => await _textViewService.MoveCaretToPosition(position);

    #endregion

    #region Window Frame Events

    public async Task SubscribeToWindowFrameEvents()
    {
        var vsUIShell = await _vsUIShell.GetServiceAsync();

        // Switch to the UI thread to ensure we can interact with the window frames.
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        vsUIShell.AdviseWindowFrameEvents(this);
    }

    public void OnFrameCreated(IVsWindowFrame frame)
    {
        // Ignore
    }

    public void OnFrameDestroyed(IVsWindowFrame frame)
    {
        // Ignore
    }

    public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible)
    {
        // Ignore
    }

    public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
    {
        // Ignore
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    public async void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        var outOfProcService = await _extensibility.ServiceBroker
            .GetProxyAsync<IOutOfProcService>(IOutOfProcService.Configuration.ServiceDescriptor, cancellationToken: default);

        try
        {
            Assumes.NotNull(outOfProcService);

            // Check if the new frame is different from the old frame
            if (oldFrame == newFrame)
            {
                await outOfProcService.ProcessActiveFrameChanged(JsonSerializer.Serialize(new DocumentView { IsFrameChange = false }));
                return;
            }

            var documentView = await _textViewService.GetDocumentView(newFrame);

            // Check if the new frame has a document view
            if (documentView == null)
            {
                await outOfProcService.ProcessActiveFrameChanged(JsonSerializer.Serialize(new DocumentView { IsFrameChange = true, IsDocumentFrame = false }));
                return;
            }

            // Notify about new document fram e with text, filepath and isDirty marker
            documentView.IsFrameChange = true;
            documentView.IsDocumentFrame = true;

            await outOfProcService.ProcessActiveFrameChanged(JsonSerializer.Serialize(documentView));
        }
        finally
        {
            (outOfProcService as IDisposable)?.Dispose();
        }
    }

    #endregion
}
