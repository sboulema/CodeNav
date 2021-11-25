using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Media;
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
            var documentView = await VS.Documents.GetActiveDocumentViewAsync();
            var caret = documentView?.TextView.Caret;
            var textBuffer2 = documentView?.TextView.TextBuffer as ITextBuffer2;

            // Subscribe to Cursor move event
            if (caret != null && !General.Instance.DisableHighlight)
            {
                var backgroundHighlightColor = await HighlightHelper.GetBackgroundHighlightColor();

                Observable
                    .FromEventPattern<CaretPositionChangedEventArgs>(
                        h => caret.PositionChanged += h,
                        h => caret.PositionChanged -= h)
                    .Select(x => x.EventArgs)
                    .Where(e => Caret_PositionChanged_LineNumberChanged(e))
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .Subscribe(e => HighlightCurrentItem(e, backgroundHighlightColor));
            }

            // Subscribe to TextBuffer changes
            if (textBuffer2 != null && General.Instance.ShowHistoryIndicators)
            {
                Observable
                    .FromEventPattern<TextContentChangedEventArgs>(
                        h => textBuffer2.ChangedOnBackground += h,
                        h => textBuffer2.ChangedOnBackground -= h)
                    .Select(x => x.EventArgs)
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .Subscribe(e => TextBuffer_ChangedOnBackground(e));
            }

            // Subscribe to Update while typing changes
            if (textBuffer2 != null && General.Instance.UpdateWhileTyping)
            {
                Observable
                    .FromEventPattern<TextContentChangedEventArgs>(
                        h => textBuffer2.ChangedOnBackground += h,
                        h => textBuffer2.ChangedOnBackground -= h)
                    .Select(x => x.EventArgs)
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .Subscribe(x => DocumentHelper.UpdateDocument(this, CodeDocumentViewModel, _column).FireAndForget());
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

        private void TextBuffer_ChangedOnBackground(TextContentChangedEventArgs e)
        {
            var changedSpans = e.Changes.Select(c => c.OldSpan);

            foreach (var span in changedSpans)
            {
                HistoryHelper.AddItemToHistory(CodeDocumentViewModel, span);
            }
        }

        private bool Caret_PositionChanged_LineNumberChanged(CaretPositionChangedEventArgs e)
        {
            var oldLineNumber = e.OldPosition.BufferPosition.GetContainingLine().LineNumber;
            var newLineNumber = e.NewPosition.BufferPosition.GetContainingLine().LineNumber;

            return oldLineNumber != newLineNumber;
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
            => ThreadHelper.JoinableTaskFactory.RunAsync(async () => await DocumentHelper.UpdateDocument(this, CodeDocumentViewModel,
                _column, null, filePath));

        public void HighlightCurrentItem(CaretPositionChangedEventArgs e, Color backgroundBrushColor)
            => HighlightHelper.HighlightCurrentItem(
                CodeDocumentViewModel,
                backgroundBrushColor,
                e.NewPosition.BufferPosition.GetContainingLine().LineNumber);
    }
}
