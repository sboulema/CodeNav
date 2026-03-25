using CodeNav.Models;
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
using System.Text.Json;

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

    /// <summary>
    /// Subscribe to outline events, so we will be notified if an outline region is changed.
    /// Get the current state of all regions
    /// </summary>
    /// <remarks>Used by the outproc service when mapping a document</remarks>
    /// <returns>JSON string with list of regions</returns>
    public async Task<string> SubscribeToRegionEvents()
    {
        // Not using context.GetActiveTextViewAsync here because VisualStudio.Extensibility doesn't support outlining yet.
        var textView = await GetCurrentTextViewAsync();

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
            var textView = await GetCurrentTextViewAsync();

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
            var textView = await GetCurrentTextViewAsync();

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

    public async Task TextViewMoveCaretToPosition(int position)
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
