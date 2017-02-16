using System;
using EnvDTE;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace CodeNav.ToolWindow
{
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [Guid("88d7674e-67d3-4835-9e0e-aa893dfc985a")]
    public class CodeNavToolWindow : ToolWindowPane
    {
        private readonly CodeViewUserControl _control;
        private WindowEvents _windowEvents;
        private DocumentEvents _documentEvents;
        private DTE _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeNavToolWindow"/> class.
        /// </summary>
        public CodeNavToolWindow() : base(null)
        {
            Caption = "CodeNav";
            _control = new CodeViewUserControl(null);
            Content = _control;
        }

        public override void OnToolWindowCreated()
        {
            var codeNavToolWindowPackage = Package as CodeNavToolWindowPackage;
            _dte = (DTE)codeNavToolWindowPackage.GetServiceHelper(typeof(DTE));

            // Wire up references for the event handlers
            _documentEvents = _dte.Events.DocumentEvents;
            _documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

            _windowEvents = _dte.Events.WindowEvents;
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;

            _control.ShowWaitingForDocument();
        }

        private void TextEditorEvents_LineChanged(TextPoint startPoint, TextPoint endPoint, int hint) => _control.HighlightCurrentItem();
        private void DocumentEvents_DocumentSaved(Document document) => UpdateDocument(document.ActiveWindow);

        private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
        {
            // Wire up reference for Caret events
            var textViewHost = GetCurrentViewHost();
            if (textViewHost != null)
            {
                textViewHost.TextView.Caret.PositionChanged += Caret_PositionChanged;
            }          

            UpdateDocument(gotFocus, gotFocus != lostFocus);
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) => _control.HighlightCurrentItem();

        private void UpdateDocument(Window window, bool forceUpdate = false)
        {
            // If the activated window does not have code we are not interested
            if (window.Document == null) return;

            _control.SetWindow(window);
            _control.UpdateDocument(forceUpdate);
        }

        private IWpfTextViewHost GetCurrentViewHost()
        {
            // code to get access to the editor's currently selected text cribbed from
            // http://msdn.microsoft.com/en-us/library/dd884850.aspx
            IVsTextManager txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
            IVsTextView vTextView = null;
            int mustHaveFocus = 1;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
            IVsUserData userData = vTextView as IVsUserData;
            if (userData == null)
            {
                return null;
            }
            else
            {
                IWpfTextViewHost viewHost;
                object holder;
                Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
                userData.GetData(ref guidViewHost, out holder);
                viewHost = (IWpfTextViewHost)holder;
                return viewHost;
            }
        }
    }
}
