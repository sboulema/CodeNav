using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;

namespace CodeNav.OutOfProc.Services;

[VisualStudioContribution]
internal class OutOfProcService : IOutOfProcService, IBrokeredService
{
    private readonly VisualStudioExtensibility _extensibility;
    private readonly CodeDocumentService _codeDocumentService;

    public OutOfProcService(
        VisualStudioExtensibility extensibility,
        CodeDocumentService codeDocumentService)
    {
        _extensibility = extensibility;
        _codeDocumentService = codeDocumentService;
    }

    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(IOutOfProcService.Configuration.ServiceName, IOutOfProcService.Configuration.ServiceVersion, typeof(OutOfProcService))
        {
            ServiceAudience = BrokeredServiceAudience.Local | BrokeredServiceAudience.Public,
        };

    public static ServiceRpcDescriptor ServiceDescriptor => IOutOfProcService.Configuration.ServiceDescriptor;

    public async Task SetCodeItemIsExpanded(int spanStart, int spanEnd, bool isExpanded)
    {
        OutliningService.SetIsExpanded(_codeDocumentService.CodeDocumentViewModel, spanStart, spanEnd, isExpanded);
    }

    public async Task DoSomethingAsync(CancellationToken cancellationToken)
    {
        await _extensibility.Shell().ShowPromptAsync("Hello from in-proc! (Showing this message from (out-of-proc)", PromptOptions.OK, cancellationToken);
    }
}
