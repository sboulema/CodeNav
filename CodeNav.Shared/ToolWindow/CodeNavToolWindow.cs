using System;
using CodeNav.Helpers;
using EnvDTE;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.TextManager.Interop;
using DefGuidList = Microsoft.VisualStudio.Editor.DefGuidList;
using AsyncTask = System.Threading.Tasks.Task;
using System.Linq;
using System.Runtime.InteropServices;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using WindowEvents = Community.VisualStudio.Toolkit.WindowEvents;
using DocumentEvents = Community.VisualStudio.Toolkit.DocumentEvents;

namespace CodeNav.ToolWindow
{
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

            RegisterEvents();

            _control.ShowWaitingForDocument();
        }

        private void RegisterEvents()
        {
            _documentEvents = VS.Events.DocumentEvents;
            _documentEvents.Saved += DocumentEvents_Saved;
            _documentEvents.Opened += DocumentEvents_Opened;

            _windowEvents = VS.Events.WindowEvents;
            _windowEvents.FrameIsVisibleChanged += WindowEvents_FrameIsVisibleChanged;
        }

        private void WindowEvents_FrameIsVisibleChanged(FrameVisibilityEventArgs obj)
            => WindowEvents_WindowActivated();


        private void DocumentEvents_Opened(object sender, string e)
            => UpdateDocument();

        private void DocumentEvents_Saved(object sender, string e)
            => UpdateDocument();

        private void OutliningManager_RegionsCollapsed(object sender, RegionsCollapsedEventArgs e)
            => _control.RegionsCollapsed(e);

        private void OutliningManager_RegionsExpanded(object sender, RegionsExpandedEventArgs e)
            => _control.RegionsExpanded(e);

        private void WindowEvents_WindowActivated()
        {
            // Wire up reference for Caret events
            var textViewHost = GetCurrentViewHost();
            if (textViewHost != null)
            {
                _control.TextView = textViewHost.TextView;
                textViewHost.TextView.Caret.PositionChanged += Caret_PositionChanged;

                if (Properties.Settings.Default.ShowHistoryIndicators)
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

            UpdateDocument();
        }

        private void TextBuffer_ChangedLowPriority(object sender, Microsoft.VisualStudio.Text.TextContentChangedEventArgs e)
        {
            var changedSpans = e.Changes.Select(c => c.OldSpan);

            foreach (var span in changedSpans)
            {
                _ = HistoryHelper.AddItemToHistory(_control.CodeDocumentViewModel, span);
            }
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) => _control.HighlightCurrentItem();

        private void UpdateDocument(bool forceUpdate = false)
        {
            try
            {
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
