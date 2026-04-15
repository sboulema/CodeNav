using CodeNav.OutOfProc.Services;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

namespace CodeNav.OutOfProc.ToolWindows;

[VisualStudioContribution]
internal class CodeNavToolWindow(CodeDocumentService codeDocumentService) : ToolWindow
{
    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.Floating,
        // TODO: Bug: This breaks a synchronous loaded hybrid extension and freezes VS during startup
        // https://github.com/sboulema/CodeNav/issues/157
        //VisibleWhen = ActivationConstraint.ClientContext(ClientContextKey.Shell.ActiveSelectionFileName, @"\.cs$"),
    };

    public override Task InitializeAsync(CancellationToken cancellationToken)
    {
        Title = "CodeNav";

        codeDocumentService.ToolWindow = this;

        return Task.CompletedTask;
    }

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        => Task.FromResult<IRemoteUserControl>(new CodeNavToolWindowControl(codeDocumentService.CodeDocumentViewModel));
}
