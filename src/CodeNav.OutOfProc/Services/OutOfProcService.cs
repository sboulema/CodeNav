using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Shell;

namespace CodeNav.OutOfProc.Services;

[VisualStudioContribution]
internal class OutOfProcService : IOutOfProcService, IBrokeredService
{
    private readonly VisualStudioExtensibility extensibility;

    public OutOfProcService(VisualStudioExtensibility extensibility)
    {
        this.extensibility = extensibility;
    }

    public static BrokeredServiceConfiguration BrokeredServiceConfiguration
        => new(IOutOfProcService.Configuration.ServiceName, IOutOfProcService.Configuration.ServiceVersion, typeof(OutOfProcService))
        {
            ServiceAudience = BrokeredServiceAudience.Local | BrokeredServiceAudience.Public,
        };

    public static ServiceRpcDescriptor ServiceDescriptor => IOutOfProcService.Configuration.ServiceDescriptor;

    public async Task DoSomethingAsync(CancellationToken cancellationToken)
    {
        await this.extensibility.Shell().ShowPromptAsync("Hello from in-proc! (Showing this message from (out-of-proc)", PromptOptions.OK, cancellationToken);
    }
}
