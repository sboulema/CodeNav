using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CodeNav.ToolWindow
{
    internal sealed class CodeNavToolWindowCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("dea24339-8fac-483b-96cb-d6f4d9bc88de");
        private readonly Package package;

        private CodeNavToolWindowCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static CodeNavToolWindowCommand Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return package;
            }
        }

        public static void Initialize(Package package)
        {
            Instance = new CodeNavToolWindowCommand(package);
        }

        private void ShowToolWindow(object sender, EventArgs e) => _ = ShowToolWindow();

        private async Task ShowToolWindow()
        {
            ToolWindowPane window = package.FindToolWindow(typeof(CodeNavToolWindow), 0, true);

            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
