using CodeNav.OutOfProc.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.OutOfProc;

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
            id: "CodeNav.dcdbcca4-3a88-432f-ba04-eb4a4cb64437",
            version: ExtensionAssemblyVersion,
            publisherName: "Samir Boulema",
            displayName: "CodeNav",
            description: "Show the code structure of your current document."),
        LoadedWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveEditorContentType, "csharp"),
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        // Add the brokered service for in-out proc communication
        serviceCollection.ProfferBrokeredService<OutOfProcService>();

        // Must be singleton so we have a single source of truth for the code document
        serviceCollection.AddSingleton<CodeDocumentService>();

        // As of now, any instance that ingests VisualStudioExtensibility is required to be added as a scoped
        // service.
        serviceCollection.AddScoped<OutputWindowService>();
        serviceCollection.AddScoped<OutliningService>();
        serviceCollection.AddScoped<WindowFrameService>();
    }
}
