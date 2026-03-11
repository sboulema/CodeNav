using CodeNav.OutOfProc.Services;
using CodeNav.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.OutOfProc
{
    /// <summary>
    /// Extension entrypoint for the VisualStudio.Extensibility extension.
    /// </summary>
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Extension
    {
        /// <inheritdoc/>
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                    id: "CodeNav.OutOfProc.e4b25f89-9445-425e-a30a-217e3dece810",
                    version: this.ExtensionAssemblyVersion,
                    publisherName: "Publisher name",
                    displayName: "CodeNav.OutOfProc",
                    description: "Extension description"),
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.ProfferBrokeredService<OutOfProcService>();

            // Must be singleton so we have a single source of truth for the code document
            serviceCollection.AddSingleton<CodeDocumentService>();

            base.InitializeServices(serviceCollection);

            // You can configure dependency injection here by adding services to the serviceCollection.
        }
    }
}
