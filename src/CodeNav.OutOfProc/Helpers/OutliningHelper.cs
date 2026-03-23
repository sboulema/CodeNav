using CodeNav.OutOfProc.Extensions;
using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.Models;
using CodeNav.OutOfProc.ViewModels;
using CodeNav.Services;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Helpers;
using System.Text.Json;

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

    /// <summary>
    /// Subscribe to region events and retrieve the current state of all regions.
    /// </summary>
    /// <remarks>
    /// Uses the InProc service.
    /// </remarks>
    /// <returns></returns>
    public async Task SubscribeToRegionEvents(CodeDocumentViewModel codeDocumentViewModel)
    {
        try
        {
            Assumes.NotNull(_inProcService);

            // Subscribe to outline region events and get all outline regions
            var outlineRegionsJsonString = await _inProcService.SubscribeToRegionEvents();

            // Synchronize all outline regions with the code items
            var outlineRegions = JsonSerializer.Deserialize<List<OutlineRegion>>(outlineRegionsJsonString);

            if (outlineRegions!.Any() != true)
            {
                return;
            }

            outlineRegions!.ForEach(outlineRegion =>
                SetIsExpanded(codeDocumentViewModel, outlineRegion.SpanStart, outlineRegion.SpanEnd, outlineRegion.IsExpanded));
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

        await codeItem
            .CodeDocumentViewModel
            .CodeDocumentService
            .OutliningHelper
            .CollapseOutlineRegion(codeItem.OutlineSpan.Start, codeItem.OutlineSpan.Length);
    }

    public static async Task ExpandOutlineRegion(CodeItem codeItem)
    {
        if (codeItem.CodeDocumentViewModel?.CodeDocumentService?.OutliningHelper == null)
        {
            return;
        }

        await codeItem
            .CodeDocumentViewModel
            .CodeDocumentService
            .OutliningHelper
            .ExpandOutlineRegion(codeItem.OutlineSpan.Start, codeItem.OutlineSpan.Length);
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

    /// <summary>
    /// Set IsExpanded property for a matching code item
    /// </summary>
    /// <remarks>A code item matches when its outline span has the same start and end as the outline region that changed</remarks>
    /// <param name="codeDocumentViewModel"></param>
    /// <param name="spanStart"></param>
    /// <param name="spanEnd"></param>
    /// <param name="isExpanded"></param>
    public static void SetIsExpanded(CodeDocumentViewModel? codeDocumentViewModel, int spanStart, int spanEnd, bool isExpanded)
    {
        codeDocumentViewModel?
            .CodeItems
            .Flatten()
            .FilterNull()
            .Where(item => item is IMembers)
            .Where(codeItem => codeItem.OutlineSpan.Start == spanStart &&
                               codeItem.OutlineSpan.End == spanEnd)
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
