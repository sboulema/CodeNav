using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using EnvDTE;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Window = EnvDTE.Window;

namespace CodeNav
{
    /// <summary>
    /// Interaction logic for CodeViewUserControl.xaml
    /// </summary>
    public partial class CodeViewUserControl
    {
        private Window _window;
        private readonly ColumnDefinition _column;
        private List<CodeItem> _cache;
        private readonly BackgroundWorker _backgroundWorker;
        internal readonly CodeDocumentViewModel CodeDocumentViewModel;
        private readonly IWpfTextView _textView;
        private readonly IOutliningManager _outliningManager;
        private VisualStudioWorkspace _workspace;

        public CodeViewUserControl(Window window, ColumnDefinition column = null, 
            IWpfTextView textView = null, IOutliningManager outliningManager = null, 
            VisualStudioWorkspace workspace = null)
        {
            InitializeComponent();

            // Setup viewmodel as datacontext
            CodeDocumentViewModel = new CodeDocumentViewModel();
            DataContext = CodeDocumentViewModel;

            // Setup backgroundworker to update datacontext
            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            _window = window;
            _column = column;
            _textView = textView;
            _outliningManager = outliningManager;
            _workspace = workspace;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public void SetWindow(Window window) => _window = window;
        public void SetWorkspace(VisualStudioWorkspace workspace) => _workspace = workspace;

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => UpdateDocument(true);

        public void SelectLine(object startLine)
        {
            int startLineAsInt;

            try
            {
                startLineAsInt = Convert.ToInt32(startLine);
            }
            catch (Exception)
            {
                LogHelper.Log($"StartLine is not a valid int for {_window.Document.Name}");
                return;
            }        

            var textSelection = _window.Document.Selection as TextSelection;
            if (textSelection == null)
            {
                LogHelper.Log($"TextSelection is null for {_window.Document.Name}");
                return;
            }

            LogHelper.Log($"GotoLine {startLineAsInt}");
            textSelection.GotoLine(startLineAsInt);
        }

        public void RegionsCollapsed(RegionsCollapsedEventArgs e) => 
            OutliningHelper.RegionsCollapsed(e, CodeDocumentViewModel.CodeDocument);

        public void RegionsExpanded(RegionsExpandedEventArgs e) =>
            OutliningHelper.RegionsExpanded(e, CodeDocumentViewModel.CodeDocument);

        public void ToggleAllRegions(bool isExpanded) =>
            OutliningHelper.SetAllRegions(CodeDocumentViewModel.CodeDocument, isExpanded);

        public void UpdateDocument(bool forceUpdate = false)
        {
            LogHelper.Log($"Starting updating document '{_window.Document.Name}'");

            if (forceUpdate)
            {
                _cache = null;
                CodeDocumentViewModel.CodeDocument.Clear();
            }

            // Do we have a cached version of this document
            if (_cache != null)
            {
                CodeDocumentViewModel.CodeDocument = _cache;
            }

            // If not show a loading item
            if (!CodeDocumentViewModel.CodeDocument.Any())
            {
                CodeDocumentViewModel.CodeDocument = CreateLoadingItem();
            }

            // Is the backgroundworker already doing something, if so stop it
            if (_backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
            }

            // Start the backgroundworker to update the list of code items
            if (!_backgroundWorker.CancellationPending)
            {
                _backgroundWorker.RunWorkerAsync(new BackgroundWorkerRequest { Document = _window.Document, ForceUpdate = forceUpdate });
            }
        }

        #region Custom Items

        private static List<CodeItem> CreateLoadingItem() => CreateItem("Loading...", KnownMonikers.Refresh);
        private static List<CodeItem> CreateSelectDocumentItem() => CreateItem("Waiting for active code document...", KnownMonikers.DocumentOutline);

        private static List<CodeItem> CreateItem(string name, ImageMoniker moniker)
        {
            return new List<CodeItem>
            {
                new CodeNamespaceItem
                {
                    Id = name,
                    Members = new List<CodeItem>
                    {
                        new CodeClassItem
                        {
                            Name = name,
                            FullName = name,
                            Id = name,
                            Foreground = new SolidColorBrush(Colors.Black),
                            BorderBrush = new SolidColorBrush(Colors.DarkGray),
                            Moniker = moniker
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Show an item to indicate that the user has to select an active code document to inspect
        /// </summary>
        public void ShowWaitingForDocument()
        {
            CodeDocumentViewModel.CodeDocument = CreateSelectDocumentItem();
        }

        #endregion

        public void HighlightCurrentItem() => HighlightHelper.HighlightCurrentItem(_window, CodeDocumentViewModel.CodeDocument);

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as BackgroundWorkerResult;

            if (result?.CodeItems == null)
            {
                LogHelper.Log($"CodeNav for '{_window.Document.Name}' updated, no results");
                return;
            }

            // Filter all null items from the code document
            SyntaxMapper.FilterNullItems(result.CodeItems);

            // Do we need to update the DataContext?
            var areEqual = AreDocumentsEqual(CodeDocumentViewModel.CodeDocument, result.CodeItems);
            if (result.ForceUpdate == false && areEqual)
            {
                LogHelper.Log($"CodeNav for '{_window.Document.Name}' updated, document did not change");
                return;
            }

            // Set the new list of codeitems as DataContext
            CodeDocumentViewModel.CodeDocument = result.CodeItems;
            _cache = result.CodeItems;

            // Set currently active codeitem
            HighlightHelper.SetForeground(CodeDocumentViewModel.CodeDocument);

            // Are there any items to show, if not hide the margin
            VisibilityHelper.SetMarginWidth(_column, CodeDocumentViewModel.CodeDocument);

            // Sync all regions
            OutliningHelper.SyncAllRegions(_outliningManager, _textView, CodeDocumentViewModel.CodeDocument);

            LogHelper.Log($"CodeNav for '{_window.Document.Name}' updated");
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_backgroundWorker.CancellationPending)
            {
                var request = e.Argument as BackgroundWorkerRequest;
                if (request == null) return;
                var codeItems = SyntaxMapper.MapDocument(request.Document, this, _workspace);
                e.Result = new BackgroundWorkerResult { CodeItems = codeItems, ForceUpdate = request.ForceUpdate };
            }
        }

        private static bool AreDocumentsEqual(List<CodeItem> existingItems, List<CodeItem> newItems)
        {
            if (existingItems == null || newItems == null) return false;
            return existingItems.SequenceEqual(newItems, new CodeItemComparer());
        }

        public void Dispose()
        {
            if (_backgroundWorker.IsBusy && _backgroundWorker.CancellationPending == false)
            {
                _backgroundWorker.CancelAsync();
            }
        }
    }
}
