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

            _ = RegisterEvents();

            _control.ShowWaitingForDocument();
        }

        private async AsyncTask RegisterEvents()
        {
            // Wire up references for the event handlers
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _documentEvents = ProjectHelper.DTE.Events.DocumentEvents;
            _documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;
            _documentEvents.DocumentOpened += DocumentEvents_DocumentOpened;

            _windowEvents = ProjectHelper.DTE.Events.WindowEvents;
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;
        }

        private void DocumentEvents_DocumentOpened(Document document)
        {
            _ = DocumentOpenedEvents(document);
        }

        private async AsyncTask DocumentOpenedEvents(Document document)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            WindowEvents_WindowActivated(document.ActiveWindow, document.ActiveWindow);
        }

        private void OutliningManager_RegionsCollapsed(object sender, RegionsCollapsedEventArgs e) =>
            _control.RegionsCollapsed(e);

        private void OutliningManager_RegionsExpanded(object sender, RegionsExpandedEventArgs e) =>
            _control.RegionsExpanded(e);

        #pragma warning disable VSTHRD100
        private async void DocumentEvents_DocumentSaved(Document document)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await UpdateDocumentAsync(document.ActiveWindow);
        }

        private async void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
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
        #pragma warning restore VSTHRD100

        private void TextBuffer_ChangedLowPriority(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            var changedSpans = e.Changes.Select(c => c.OldSpan);

            foreach (var span in changedSpans)
            {
                _ = HistoryHelper.AddItemToHistory(_control.CodeDocumentViewModel, span);
            }
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) => _ = CaretPositionChanged();

        private async AsyncTask CaretPositionChanged() => await _control.HighlightCurrentItem();

        private async AsyncTask UpdateDocumentAsync(Window window, bool forceUpdate = false)
        {
            try
            {
                // If the activated window does not have code we are not interested
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (window == null || window.Document == null)
                {
                    return;
                }

                _control.SetWorkspace(_workspace);
                _ = _control.UpdateDocument(forceUpdate);
            }
            catch (Exception e)
            {
                LogHelper.Log("Error updating document in ToolWindow", e);
            }
        }

        private IWpfTextViewHost GetCurrentViewHost()
        {
            // code to get access to the editor's currently selected text cribbed from
            // http://msdn.microsoft.com/en-us/library/dd884850.aspx

            var txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
            var mustHaveFocus = 1;
            txtMgr.GetActiveView(mustHaveFocus, null, out IVsTextView vTextView);

            if (!(vTextView is IVsUserData userData))
            {
                return null;
            }

            IWpfTextViewHost viewHost;
            Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
            userData.GetData(ref guidViewHost, out var holder);
            viewHost = (IWpfTextViewHost)holder;

            return viewHost;
        }
    }
}
