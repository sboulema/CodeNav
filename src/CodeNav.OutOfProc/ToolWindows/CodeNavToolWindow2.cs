using CodeNav.OutOfProc.Services;
using CodeNav.OutOfProc.ViewModels;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;

namespace CodeNav.OutOfProc.ToolWindows;

[VisualStudioContribution]
internal class CodeNavToolWindow2(CodeDocumentService codeDocumentService) : ToolWindow
{
    private CodeDocumentViewModel? codeDocumentViewModel;

    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.Floating,
    };

    public override async Task InitializeAsync(CancellationToken cancellationToken)
    {
        Title = "CodeNav 2";

        codeDocumentViewModel = codeDocumentService.RegisterWindow(this);

        // Immediately show whatever document is currently active,
        // instead of leaving the window empty because we are opening a duplicated window.
        await codeDocumentService.RefreshWindow(codeDocumentViewModel, cancellationToken);
    }

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        => Task.FromResult<IRemoteUserControl>(new CodeNavToolWindowControl(codeDocumentViewModel));
}
