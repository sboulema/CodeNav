using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Mappers;
using CodeNav.Models;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Window = EnvDTE.Window;

namespace CodeNav
{
    /// <summary>
    /// Interaction logic for CodeViewUserControl.xaml
    /// </summary>
    public partial class CodeViewUserControl
    {
        private Window _window;
        private ColumnDefinition _column;
        private List<CodeItem> _cache;
        private readonly BackgroundWorker _backgroundWorker;
        internal readonly CodeDocumentViewModel CodeDocumentViewModel;

        public CodeViewUserControl(Window window, ColumnDefinition column = null)
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

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public void SetWindow(Window window) => _window = window;

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => UpdateDocument(true);

        public void SelectLine(TextPoint textPoint)
        {
            var textSelection = _window.Document.Selection as TextSelection;
            if (textSelection == null)
            {
                LogHelper.Log($"TextSelection is null for {_window.Document.FullName}");
                return;
            }

            textSelection.MoveToPoint(textPoint);
        }

        public void UpdateDocument(bool forceUpdate = false)
        {
            // Do we have code items in the text document
            var elements = _window.ProjectItem?.FileCodeModel?.CodeElements;
            if (elements == null) return;

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
                _backgroundWorker.RunWorkerAsync(new BackgroundWorkerRequest { Elements = elements, ForceUpdate = forceUpdate });
            }
        }

        #region Custom Items

        private static List<CodeItem> CreateLoadingItem() => CreateItem("Loading...", "Refresh");
        private static List<CodeItem> CreateSelectDocumentItem() => CreateItem("Waiting for active code document...", "DocumentOutline");

        private static List<CodeItem> CreateItem(string name, string icon)
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
                            IconPath = $"pack://application:,,,/CodeNav;component/Icons/UI/{icon}_16x.xaml"
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = e.Result as BackgroundWorkerResult;

            if (result == null) return;

            // Filter all null items from the code document
            CodeItemMapper.FilterNullItems(result.CodeItems);

            // Do we need to update the DataContext?
            var areEqual = AreDocumentsEqual(CodeDocumentViewModel.CodeDocument, result.CodeItems);
            if (result.ForceUpdate == false && areEqual)
            {
                stopwatch.Stop();
                LogHelper.Log($"RunWorkerCompleted in {stopwatch.ElapsedMilliseconds} ms, document did not change");
                return;
            }

            // Set the new list of codeitems as DataContext
            CodeDocumentViewModel.CodeDocument = result.CodeItems;
            _cache = result.CodeItems;

            // Set currently active codeitem
            HighlightHelper.SetForeground(CodeDocumentViewModel.CodeDocument);

            // Are there any items to show, if not hide the margin
            VisibilityHelper.SetMarginWidth(_column, CodeDocumentViewModel.CodeDocument);

            stopwatch.Stop();
            LogHelper.Log($"RunWorkerCompleted in {stopwatch.ElapsedMilliseconds} ms");
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!_backgroundWorker.CancellationPending)
            {
                var request = e.Argument as BackgroundWorkerRequest;
                var codeItems = CodeItemMapper.MapDocument(request.Elements, this);
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
