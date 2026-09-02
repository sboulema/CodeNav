using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace CodeNav.OutOfProc.ToolWindows;

[VisualStudioContribution]
public class CodeNavToolWindowCommand2 : Command
{
    public override CommandConfiguration CommandConfiguration => new("%CodeNav.CodeNavToolWindowCommand2.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.DocumentOutline, IconSettings.IconAndText),
    };

    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        return base.InitializeAsync(cancellationToken);
    }

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        => await Extensibility.Shell().ShowToolWindowAsync<CodeNavToolWindow2>(activate: true, cancellationToken);
}
