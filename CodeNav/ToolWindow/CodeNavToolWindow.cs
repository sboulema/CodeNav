using System;
using CodeNav.Helpers;
using EnvDTE;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using DefGuidList = Microsoft.VisualStudio.Editor.DefGuidList;
using AsyncTask = System.Threading.Tasks.Task;

namespace CodeNav.ToolWindow
{
    using System.Linq;
    using System.Runtime.InteropServices;
    using CodeNav.Properties;
    using Microsoft.VisualStudio.Shell;

    [Guid("88d7674e-67d3-4835-9e0e-aa893dfc985a")]
    public class CodeNavToolWindow : ToolWindowPane
    {
        private readonly CodeViewUserControl _control;
        private WindowEvents _windowEvents;
        private DocumentEvents _documentEvents;
        private VisualStudioWorkspace _workspace;

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
            _workspace = codeNavToolWindowPackage.ComponentModel.GetService<VisualStudioWorkspace>();

            if (_control.Dte == null && codeNavToolWindowPackage.DTE != null)
            {
                _control.Dte = codeNavToolWindowPackage.DTE;
            }        

            // Wire up references for the event handlers
            _documentEvents = codeNavToolWindowPackage.DTE.Events.DocumentEvents;
            _documentEvents.DocumentSaved += DocumentEvents_DocumentSavedAsync;
            _documentEvents.DocumentOpened += DocumentEvents_DocumentOpened;

            _windowEvents = codeNavToolWindowPackage.DTE.Events.WindowEvents;
            _windowEvents.WindowActivated += WindowEvents_WindowActivatedAsync;

            _control.ShowWaitingForDocument();
        }

        private void DocumentEvents_DocumentOpened(Document document) 
            => WindowEvents_WindowActivatedAsync(document.ActiveWindow, document.ActiveWindow);

        private void OutliningManager_RegionsCollapsed(object sender, RegionsCollapsedEventArgs e) =>
            _control.RegionsCollapsed(e);

        private void OutliningManager_RegionsExpanded(object sender, RegionsExpandedEventArgs e) =>
            _control.RegionsExpanded(e);

        private async void DocumentEvents_DocumentSavedAsync(Document document) => await UpdateDocumentAsync(document.ActiveWindow);

        private async void WindowEvents_WindowActivatedAsync(Window gotFocus, Window lostFocus)
        {
            // Wire up reference for Caret events
            var textViewHost = GetCurrentViewHost();
            if (textViewHost != null)
            {
                _control.TextView = textViewHost.TextView;
                textViewHost.TextView.Caret.PositionChanged += Caret_PositionChanged;

                if (Settings.Default.ShowHistoryIndicators)
                {
                    textViewHost.TextView.TextBuffer.ChangedLowPriority += TextBuffer_ChangedLowPriority;
                }

                // Subscribe to Outlining events
                var outliningManagerService = OutliningHelper.GetOutliningManagerService(Package as IServiceProvider);
                var outliningManager = OutliningHelper.GetOutliningManager(outliningManagerService, GetCurrentViewHost().TextView);

                if (outliningManager != null && outliningManagerService != null)
                {
                    _control.OutliningManagerService = outliningManagerService;
                    outliningManager.RegionsExpanded -= OutliningManager_RegionsExpanded;
                    outliningManager.RegionsExpanded += OutliningManager_RegionsExpanded;
                    outliningManager.RegionsCollapsed -= OutliningManager_RegionsCollapsed;
                    outliningManager.RegionsCollapsed += OutliningManager_RegionsCollapsed;
                }
            }

            await UpdateDocumentAsync(gotFocus, gotFocus != lostFocus);
        }

        private void TextBuffer_ChangedLowPriority(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            var changedSpans = e.Changes.Select(c => c.OldSpan);

            foreach (var span in changedSpans)
            {
                HistoryHelper.AddItemToHistory(_control.CodeDocumentViewModel, span);
            }
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) => _control.HighlightCurrentItem();

        private async AsyncTask UpdateDocumentAsync(Window window, bool forceUpdate = false)
        {
            // If the activated window does not have code we are not interested
            if (window.Document == null) return;

            _control.SetWindow(window);
            _control.SetWorkspace(_workspace);
            await _control.UpdateDocumentAsync(forceUpdate);
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
