using CodeNav.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav;

/// <summary>
/// Extension entry point for the VisualStudio.Extensibility extension.
/// </summary>
[VisualStudioContribution]
internal class CodeNavExtension : Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        LoadedWhen = ActivationConstraint.SolutionState(SolutionState.FullyLoaded),
        Metadata = new(
            id: "CodeNav.dcdbcca4-3a88-432f-ba04-eb4a4cb64437",
            version: ExtensionAssemblyVersion,
            publisherName: "Samir Boulema",
            displayName: "CodeNav",
            description: "Show the code structure of your current document.")
        {
            MoreInfo = "https://github.com/sboulema/CodeNav",
            ReleaseNotes = "https://github.com/sboulema/Codenav/releases",
            Tags = ["code", "structure", "navigation", "map", "codemap"],
            // TODO: BUG? License file is missing
            //License = "Resources/License.txt",
            Icon = "Resources/Logo_90x90.png",
            PreviewImage = "Resources/Screenshot.png",
            InstallationTargetVersion = "[17.14,19.0)",
        },
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSettingsObservers();

        // Must be singleton so we have a single source of truth for the code document
        serviceCollection.AddSingleton<CodeDocumentService>();

        // As of now, any instance that ingests VisualStudioExtensibility is required to be added as a scoped
        // service.

        base.InitializeServices(serviceCollection);
    }
}