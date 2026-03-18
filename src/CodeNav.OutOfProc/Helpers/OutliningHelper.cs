using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.ViewModels;
using CodeNav.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Helpers;

namespace CodeNav.OutOfProc.Helpers;

public class OutliningHelper : DisposableObject
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly Task _initializationTask;
    private IInProcService? _inProcService;

    public OutliningHelper(VisualStudioExtensibility extensibility)
    {
        _extensibility = extensibility;
        _initializationTask = Task.Run(InitializeAsync);
    }

    public async Task SubscribeToRegionEvents()
    {
        try
        {
            Assumes.NotNull(_inProcService);
            await _inProcService.SubscribeToRegionEvents();
        }
        catch (Exception e)
        {
            // TODO: Add logging
        }
    }

    public async Task CollapseOutlineRegion(int start, int length)
    {
        try
        {
            Assumes.NotNull(_inProcService);
            await _inProcService.CollapseOutlineRegion(start, length);
        }
        catch (Exception e)
        {
            // TODO: Add logging
        }
    }

    public async Task ExpandOutlineRegion(int start, int length)
    {
        try
        {
            Assumes.NotNull(_inProcService);
            await _inProcService.ExpandOutlineRegion(start, length);
        }
        catch (Exception e)
        {
            // TODO: Add logging
        }
    }

    public static async Task CollapseOutlineRegion(CodeItem codeItem)
    {
        if (codeItem.CodeDocumentViewModel?.CodeDocumentService?.OutliningHelper == null)
        {
            return;
        }

        await codeItem.CodeDocumentViewModel.CodeDocumentService.OutliningHelper.CollapseOutlineRegion(codeItem.Span.Start, codeItem.Span.Length);
    }

    public static async Task ExpandOutlineRegion(CodeItem codeItem)
    {
        if (codeItem.CodeDocumentViewModel?.CodeDocumentService?.OutliningHelper == null)
        {
            return;
        }

        await codeItem.CodeDocumentViewModel.CodeDocumentService.OutliningHelper.ExpandOutlineRegion(codeItem.Span.Start, codeItem.Span.Length);
    }

    /// <summary>
    /// Set IsExpanded property to false on all code items
    /// </summary>
    /// <remarks>Used in the main toolbar and in the code item context menu</remarks>
    /// <param name="codeDocumentViewModel">The code document view model whose nodes will be collapsed.</param>
    public static void CollapseAll(CodeDocumentViewModel? codeDocumentViewModel)
        => SetAllIsExpanded(codeDocumentViewModel, isExpanded: false);

    /// <summary>
    /// Set IsExpanded property to true on all code items
    /// </summary>
    /// <remarks>Used in the main toolbar and in the code item context menu</remarks>
    /// <param name="codeDocumentViewModel">The code document view model whose nodes will be expanded.</param>
    public static void ExpandAll(CodeDocumentViewModel? codeDocumentViewModel)
        => SetAllIsExpanded(codeDocumentViewModel, isExpanded: true);

    public static void SetIsExpanded(CodeDocumentViewModel? codeDocumentViewModel, int spanStart, int spanEnd, bool isExpanded)
    {
        codeDocumentViewModel?
            .CodeItems
            .Flatten()
            .FilterNull()
            .Where(item => item is IMembers)
            .Where(codeItem => codeItem.Span.Start == spanStart ||
                               codeItem.Span.End == spanEnd)
            .Cast<IMembers>()
            .ToList()
            .ForEach(codeItem => codeItem.IsExpanded = isExpanded);
    }

    /// <summary>
    /// Sets the expanded state for all member items within the specified code document view model.
    /// </summary>
    /// <remarks>Only items that implement the IMembers interface are affected.</remarks>
    /// <param name="codeDocumentViewModel">The code document view model containing the code items to update. If null, no action is taken.</param>
    /// <param name="isExpanded">A value indicating whether the member items should be expanded (<see langword="true"/>) or collapsed (<see
    /// langword="false"/>).</param>
    private static void SetAllIsExpanded(CodeDocumentViewModel? codeDocumentViewModel, bool isExpanded)
    {
        codeDocumentViewModel?
            .CodeItems
            .Flatten()
            .FilterNull()
            .Where(item => item is IMembers)
            .Cast<IMembers>()
            .ToList()
            .ForEach(item => item.IsExpanded = isExpanded);
    }

    private async Task InitializeAsync()
    {
        (_inProcService as IDisposable)?.Dispose();
        _inProcService = await _extensibility
            .ServiceBroker
            .GetProxyAsync<IInProcService>(IInProcService.Configuration.ServiceDescriptor, cancellationToken: default);
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        if (isDisposing)
        {
            (_inProcService as IDisposable)?.Dispose();
        }
    }
}
