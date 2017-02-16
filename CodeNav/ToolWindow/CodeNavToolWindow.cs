using EnvDTE;

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
        private TextEditorEvents _textEditorEvents;
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

            _textEditorEvents = _dte.Events.TextEditorEvents;
            _textEditorEvents.LineChanged += TextEditorEvents_LineChanged;

            _windowEvents = _dte.Events.WindowEvents;
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;
        }

        private void TextEditorEvents_LineChanged(TextPoint startPoint, TextPoint endPoint, int hint) => _control.HighlightCurrentItem();
        private void DocumentEvents_DocumentSaved(Document document) => UpdateDocument(document.ActiveWindow);
        private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus) => UpdateDocument(gotFocus);

        private void UpdateDocument(Window window)
        {
            // If the activated window does not have code we are not interested
            if (window.Document == null) return;

            _control.SetWindow(window);
            _control.UpdateDocument();
        }
    }
}
