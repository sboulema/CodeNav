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

        // Conditions:
        // - Frame is null
        // - Frame did not change
        // - Frame is not a document frame
        // Actions:
        // - Do nothing
        if (documentView?.IsDocumentFrame != true)
        {
            return;
        }

        // Frame has changed and has a text document, so we need to update the list of code items
        await codeDocumentService.UpdateCodeDocumentViewModel(
            extensibility,
            documentView.FilePath,
            documentView.Text,
            default);
    }

    public async Task DoSomethingAsync(CancellationToken cancellationToken)
    {
        await extensibility.Shell().ShowPromptAsync("Hello from in-proc! (Showing this message from (out-of-proc)", PromptOptions.OK, cancellationToken);
    }
}
