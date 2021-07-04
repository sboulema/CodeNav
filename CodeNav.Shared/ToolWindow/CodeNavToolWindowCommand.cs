using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CodeNav.ToolWindow
{
    [Command(PackageGuids.guidCodeNavToolWindowPackageString, PackageIds.CodeNavToolWindowCommandId)]
    internal sealed class CodeNavToolWindowCommand : BaseCommand<CodeNavToolWindowCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
            => await CodeNavToolWindow.ShowAsync();
    }
}
