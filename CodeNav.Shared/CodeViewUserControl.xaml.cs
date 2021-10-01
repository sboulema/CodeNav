using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Task = System.Threading.Tasks.Task;

namespace CodeNav
{
    public partial class CodeViewUserControl : ICodeViewUserControl
    {
        private readonly ColumnDefinition _column;
        private IOutliningManager _outliningManager;
        public CodeDocumentViewModel CodeDocumentViewModel { get; set; }
        
        public CodeViewUserControl(ColumnDefinition column = null)
        {
            InitializeComponent();

            // Setup viewmodel as datacontext
            CodeDocumentViewModel = new CodeDocumentViewModel();
            DataContext = CodeDocumentViewModel;

            _column = column;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
            VS.Events.DocumentEvents.Opened += DocumentEvents_Opened;
            VS.Events.DocumentEvents.Saved += DocumentEvents_Saved;
            VS.Events.WindowEvents.ActiveFrameChanged += WindowEvents_ActiveFrameChanged;
        }

        public async Task RegisterDocumentEvents()
        {
            // Subscribe to Cursor move event
            var documentView = await VS.Documents.GetActiveDocumentViewAsync();

            if (documentView?.TextView.Caret != null &&
                !General.Instance.DisableHighlight)
            {
                documentView.TextView.Caret.PositionChanged -= Caret_PositionChanged;
                documentView.TextView.Caret.PositionChanged += Caret_PositionChanged;
            }

            // Subscribe to TextBuffer changes
            if ((documentView?.TextView.TextBuffer as ITextBuffer2) != null &&
                General.Instance.ShowHistoryIndicators)
            {
                var textBuffer2 = documentView.TextView.TextBuffer as ITextBuffer2;

                textBuffer2.ChangedOnBackground -= TextBuffer_ChangedOnBackground;
                textBuffer2.ChangedOnBackground += TextBuffer_ChangedOnBackground;
            }

            // Subscribe to Update while typing changes
            if ((documentView?.TextView.TextBuffer as ITextBuffer2) != null &&
                General.Instance.UpdateWhileTyping)
            {
                var textBuffer2 = documentView.TextView.TextBuffer as ITextBuffer2;

                Observable
                    .FromEventPattern<TextContentChangedEventArgs>(
                        h => textBuffer2.ChangedOnBackground += h,
                        h => textBuffer2.ChangedOnBackground -= h)
                    .Select(x => x.EventArgs)
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .Subscribe(x => UpdateDocument());
            }

            // Subscribe to Outlining events
            _outliningManager = await OutliningHelper.GetOutliningManager();
            if (_outliningManager != null)
            {
                _outliningManager.RegionsExpanded -= OutliningManager_RegionsExpanded;
                _outliningManager.RegionsExpanded += OutliningManager_RegionsExpanded;
                _outliningManager.RegionsCollapsed -= OutliningManager_RegionsCollapsed;
                _outliningManager.RegionsCollapsed += OutliningManager_RegionsCollapsed;
            }
        }

        private void DocumentEvents_Opened(string obj)
        {
            RegisterDocumentEvents().FireAndForget();
        }

        private void WindowEvents_ActiveFrameChanged(ActiveFrameChangeEventArgs obj)
            => WindowChangedEvent(obj).FireAndForget();

        private void DocumentEvents_Saved(string e)
            => UpdateDocument();

        private async Task WindowChangedEvent(ActiveFrameChangeEventArgs obj)
        {
            if (obj.OldFrame == obj.NewFrame)
            {
                return;
            }

            var documentView = await obj.NewFrame.GetDocumentViewAsync();

            var filePath = documentView?.Document?.FilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            RegisterDocumentEvents().FireAndForget();

            UpdateDocument(filePath);
        }

        private void TextBuffer_ChangedOnBackground(object sender, TextContentChangedEventArgs e)
        {
            var changedSpans = e.Changes.Select(c => c.OldSpan);

            foreach (var span in changedSpans)
            {
                HistoryHelper.AddItemToHistory(CodeDocumentViewModel, span);
            }
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            var oldLineNumber = e.OldPosition.BufferPosition.GetContainingLine().LineNumber;
            var newLineNumber = e.NewPosition.BufferPosition.GetContainingLine().LineNumber;

            if (oldLineNumber == newLineNumber)
            {
                return;
            }

            HighlightCurrentItem(newLineNumber);
        }

        private void OutliningManager_RegionsCollapsed(object sender, RegionsCollapsedEventArgs e)
            => OutliningHelper.RegionsCollapsed(e, CodeDocumentViewModel.CodeDocument);

        private void OutliningManager_RegionsExpanded(object sender, RegionsExpandedEventArgs e)
            => OutliningHelper.RegionsExpanded(e, CodeDocumentViewModel.CodeDocument);

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => UpdateDocument();

        public void FilterBookmarks()
            => VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel);

        public void ToggleAll(bool isExpanded, List<CodeItem> root = null)
            => OutliningHelper.ToggleAll(root ?? CodeDocumentViewModel.CodeDocument, isExpanded);

        public void UpdateDocument(string filePath = "")
            => DocumentHelper.UpdateDocument(this, CodeDocumentViewModel,
                _column, null, filePath).FireAndForget();

        public void HighlightCurrentItem(int lineNumber)
            => HighlightHelper.HighlightCurrentItem(CodeDocumentViewModel, lineNumber).FireAndForget();
    }
}
