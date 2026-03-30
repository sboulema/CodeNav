using CodeNav.OutOfProc.Helpers;
using CodeNav.OutOfProc.Models;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Text.Json;

namespace CodeNav.OutOfProc.Services;

[VisualStudioContribution]
internal class OutOfProcService(
    VisualStudioExtensibility extensibility,
    CodeDocumentService codeDocumentService) : IOutOfProcService, IBrokeredService
{
    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(IOutOfProcService.Configuration.ServiceName, IOutOfProcService.Configuration.ServiceVersion, typeof(OutOfProcService))
        {
            ServiceAudience = BrokeredServiceAudience.Local | BrokeredServiceAudience.Public,
        };

    public static ServiceRpcDescriptor ServiceDescriptor => IOutOfProcService.Configuration.ServiceDescriptor;

    public async Task SetCodeItemIsExpanded(int spanStart, int spanEnd, bool isExpanded)
    {
        OutliningService.SetIsExpanded(codeDocumentService.CodeDocumentViewModel, spanStart, spanEnd, isExpanded);
    }

    public async Task ProcessActiveFrameChanged(string documentViewJsonString)
    {
        var documentView = JsonSerializer.Deserialize<DocumentView>(documentViewJsonString);

        // Frame is null or not a frame change, so we do not have to do anything
        if (documentView?.IsFrameChange != true)
        {
            return;
        }

        if (!documentView.IsDocumentFrame != true)
        {
            // Frame is not a document frame, so we have to clear the list of code items
            codeDocumentService.CodeDocumentViewModel.CodeItems = PlaceholderHelper.CreateSelectDocumentItem();
            return;
        }

        // Frame has changed and has a text document, so we need to update the list of code items
        // TODO: Update the list of code items with the new document view
    }

    public async Task DoSomethingAsync(CancellationToken cancellationToken)
    {
        await extensibility.Shell().ShowPromptAsync("Hello from in-proc! (Showing this message from (out-of-proc)", PromptOptions.OK, cancellationToken);
    }
}
