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
using CodeNav.Properties;
using Microsoft.CodeAnalysis.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace CodeNav
{
    /// <summary>
    /// Interaction logic for CodeViewUserControl.xaml
    /// </summary>
    public partial class CodeViewUserControl : ICodeViewUserControl
    {
        private Window _window;
        private readonly ColumnDefinition _column;
        private List<CodeItem> _cache;
        public CodeDocumentViewModel CodeDocumentViewModel { get; set; }
        internal IWpfTextView TextView;
        internal IOutliningManagerService OutliningManagerService;
        private VisualStudioWorkspace _workspace;
        private CodeNavMargin _margin;
        public _DTE Dte { get; set; }

        public CodeViewUserControl(Window window, ColumnDefinition column = null, 
            IWpfTextView textView = null, IOutliningManagerService outliningManagerService = null, 
            VisualStudioWorkspace workspace = null, CodeNavMargin margin = null, _DTE dte = null)
        {
            InitializeComponent();

            // Setup viewmodel as datacontext
            CodeDocumentViewModel = new CodeDocumentViewModel();
            DataContext = CodeDocumentViewModel;

            _window = window;
            _column = column;
            TextView = textView;
            OutliningManagerService = outliningManagerService;
            _workspace = workspace;
            _margin = margin;
            Dte = dte;

            LogHelper.Dte = dte;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public void SetWindow(Window window) => _window = window;
        public void SetWorkspace(VisualStudioWorkspace workspace) => _workspace = workspace;

        #pragma warning disable VSTHRD100
        private async void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => await UpdateDocumentAsync(true);
        #pragma warning restore VSTHRD100

        public void SelectLine(object startLinePosition, bool extend = false)
        {
            int line;
            int offset;

            try
            {
                var linePosition = (LinePosition)startLinePosition;
                line = linePosition.Line + 1;
                offset = linePosition.Character + 1;
            }
            catch (Exception)
            {
                // StartLine is not a valid int for document
                return;
            }

            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            var textSelection = _window.Document.Selection as TextSelection;
            if (textSelection == null)
            {
                // TextSelection is null for document
                return;
            }

            try
            {
                textSelection.MoveToLineAndOffset(line, offset, extend);

                var tp = (TextPoint)textSelection.TopPoint;
                tp.TryToShow(vsPaneShowHow.vsPaneShowCentered, null);
            }
            catch (Exception)
            {
                // GotoLine failed
                return;
            }   
        }

        public void Select(object startLinePosition, object endLinePosition)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            SelectLine(startLinePosition);
            SelectLine(endLinePosition, true);
        }

        public void FilterBookmarks() 
            => VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel);

        public void RegionsCollapsed(RegionsCollapsedEventArgs e) => 
            OutliningHelper.RegionsCollapsed(e, CodeDocumentViewModel.CodeDocument);

        public void RegionsExpanded(RegionsExpandedEventArgs e) =>
            OutliningHelper.RegionsExpanded(e, CodeDocumentViewModel.CodeDocument);

        public void ToggleAll(bool isExpanded, List<CodeItem> root = null)
        {
            OutliningHelper.ToggleAll(root ?? CodeDocumentViewModel.CodeDocument, isExpanded);
        }

        public async Task UpdateDocumentAsync(bool forceUpdate = false)
        {
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var activeDocument = DocumentHelper.GetActiveDocument(Dte);

            if (activeDocument == null) return;

            CodeDocumentViewModel.FilePath = activeDocument.FullName;

            // Do we need to change the side where the margin is displayed
            if (_margin?.MarginSide != null &&
                _margin?.MarginSide != Settings.Default.MarginSide &&
                Dte != null)
            {
                var filename = activeDocument.FullName;
                Dte.ExecuteCommand("File.Close");
                Dte.ExecuteCommand("File.OpenFile", filename);
            }

            try
            {
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

                var codeItems = await SyntaxMapper.MapDocumentAsync(activeDocument, this, _workspace);

                if (codeItems == null)
                {
                    // CodeNav for document updated, no results
                    return;
                }

                // Filter all null items from the code document
                SyntaxMapper.FilterNullItems(codeItems);

                // Sort items
                CodeDocumentViewModel.SortOrder = Settings.Default.SortOrder;
                SortHelper.Sort(codeItems, Settings.Default.SortOrder);

                // Set currently active codeitem
                HighlightHelper.SetForeground(codeItems);

                // Set the new list of codeitems as DataContext
                CodeDocumentViewModel.CodeDocument = codeItems;
                _cache = codeItems;

                // Apply current visibility settings to the document
                VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel);

                // Apply bookmarks
                LoadBookmarksFromStorage();
                BookmarkHelper.ApplyBookmarks(CodeDocumentViewModel, Dte?.Solution?.FileName);

                // Apply history items
                LoadHistoryItemsFromStorage();
                HistoryHelper.ApplyHistoryIndicator(CodeDocumentViewModel);
            }
            catch (Exception e)
            {
                LogHelper.Log("Error running UpdateDocument", e);
            }

            try
            {
                // Sync all regions
                OutliningHelper.SyncAllRegions(OutliningManagerService, TextView, CodeDocumentViewModel.CodeDocument);

                // Should the margin be shown and are there any items to show, if not hide the margin
                VisibilityHelper.SetMarginWidth(_column, CodeDocumentViewModel.CodeDocument);
            }
            catch (Exception e)
            {
                LogHelper.Log("Error finishing UpdateDocument", e);
            }
        }

        public bool IsLargeDocument()
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            return DocumentHelper.GetNumberOfLines(Dte.ActiveDocument) > Settings.Default.AutoLoadLineThreshold && Settings.Default.AutoLoadLineThreshold > 0;
        }

        #region Custom Items

        private static List<CodeItem> CreateLoadingItem() => CreateItem("Loading...", KnownMonikers.Refresh);
        private static List<CodeItem> CreateSelectDocumentItem() => CreateItem("Waiting for active code document...", KnownMonikers.DocumentOutline);
        public List<CodeItem> CreateLineThresholdPassedItem() => CreateItem("Click Refresh to load file...", KnownMonikers.DocumentError);

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
                            ForegroundColor = Colors.Black,
                            BorderColor = Colors.DarkGray,
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

        public void HighlightCurrentItem()
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
            HighlightHelper.HighlightCurrentItem(_window, CodeDocumentViewModel);
        }

        private static bool AreDocumentsEqual(List<CodeItem> existingItems, List<CodeItem> newItems)
        {
            if (existingItems == null || newItems == null) return false;
            return existingItems.SequenceEqual(newItems, new CodeItemComparer());
        }

        private void LoadBookmarksFromStorage()
        {
            if (string.IsNullOrEmpty(Dte?.Solution?.FileName)) return;

            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            var solutionStorage = SolutionStorageHelper.Load<SolutionStorageModel>(Dte.Solution.FileName);

            if (solutionStorage?.Documents == null) return;

            var storageItem = solutionStorage.Documents
                .FirstOrDefault(s => s.FilePath.Equals(CodeDocumentViewModel.FilePath));
            if (storageItem != null)
            {
                CodeDocumentViewModel.Bookmarks = storageItem.Bookmarks;
            }
        }

        private void LoadHistoryItemsFromStorage()
        {
            if (string.IsNullOrEmpty(Dte?.Solution?.FileName)) return;

            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            var solutionStorage = SolutionStorageHelper.Load<SolutionStorageModel>(Dte.Solution.FileName);

            if (solutionStorage?.Documents == null) return;

            var storageItem = solutionStorage.Documents
                .FirstOrDefault(s => s.FilePath.Equals(CodeDocumentViewModel.FilePath));
            if (storageItem != null)
            {
                CodeDocumentViewModel.HistoryItems = storageItem.HistoryItems;
            }
        }
    }
}
