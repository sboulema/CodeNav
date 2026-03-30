using CodeNav.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav
{
    /// <summary>
    /// Extension entrypoint for the VisualStudio.Extensibility extension.
    /// </summary>
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Extension
    {
        /// <inheritdoc />
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            RequiresInProcessHosting = true,
            LoadedWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, "csharp"),
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.ProfferBrokeredService(InProcService.BrokeredServiceConfiguration, IInProcService.Configuration.ServiceDescriptor);

            base.InitializeServices(serviceCollection);

            // As of now, any instance that ingests VisualStudioExtensibility is required to be added as a scoped
            // service.
            serviceCollection.AddScoped<TextViewService>();
        }
    }
}
