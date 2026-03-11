using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace CodeNav.OutOfProc.ToolWindows;

[VisualStudioContribution]
public class CodeNavToolWindowCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("%CodeNav.CodeNavToolWindowCommand.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ViewOtherWindowsMenu],
        Icon = new(ImageMoniker.KnownValues.DocumentOutline, IconSettings.IconAndText),
    };

    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        return base.InitializeAsync(cancellationToken);
    }

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        => await Extensibility.Shell().ShowToolWindowAsync<CodeNavToolWindow>(activate: true, cancellationToken);
}
