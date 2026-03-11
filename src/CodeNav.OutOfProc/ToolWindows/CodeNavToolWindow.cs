using CodeNav.OutOfProc.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

namespace CodeNav.OutOfProc.ToolWindows;

[VisualStudioContribution]
internal class CodeNavToolWindow(CodeDocumentService documentService) : ToolWindow
{
    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.Floating,
        VisibleWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveSelectionFileName, @"\.cs$"),
    };

    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        Title = "CodeNav";

        return Task.CompletedTask;
    }

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        => Task.FromResult<IRemoteUserControl>(new CodeNavToolWindowControl(documentService.CodeDocumentViewModel));
}
