using System;
using CodeNav.Helpers;
using System.Runtime.InteropServices;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace CodeNav.ToolWindow
{
    public class CodeNavToolWindow : BaseToolWindow<CodeNavToolWindow>
    {
        private CodeViewUserControl _control;

        public override string GetTitle(int toolWindowId) => "CodeNav";

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();

            _control = new CodeViewUserControl();

            _control.tag = "toolwindow";
            //_control.CodeDocumentViewModel.CodeDocument = PlaceholderHelper.CreateSelectDocumentItem();

            return _control;
        }

        [Guid("88d7674e-67d3-4835-9e0e-aa893dfc985a")]
        public class Pane : ToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.DocumentOutline;
            }
        }
    }
}
