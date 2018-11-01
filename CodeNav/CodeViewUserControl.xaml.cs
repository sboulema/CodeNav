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
    public partial class CodeViewUserControl
    {
        private Window _window;
        private readonly ColumnDefinition _column;
        private List<CodeItem> _cache;
        internal readonly CodeDocumentViewModel CodeDocumentViewModel;
        internal IWpfTextView TextView;
        internal IOutliningManager OutliningManager;
        private VisualStudioWorkspace _workspace;
        private CodeNavMargin _margin;
        public DTE Dte;

        public CodeViewUserControl(Window window, ColumnDefinition column = null, 
            IWpfTextView textView = null, IOutliningManager outliningManager = null, 
            VisualStudioWorkspace workspace = null, CodeNavMargin margin = null, DTE dte = null)
        {
            InitializeComponent();

            // Setup viewmodel as datacontext
            CodeDocumentViewModel = new CodeDocumentViewModel();
            DataContext = CodeDocumentViewModel;

            _window = window;
            _column = column;
            TextView = textView;
            OutliningManager = outliningManager;
            _workspace = workspace;
            _margin = margin;
            Dte = dte;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public void SetWindow(Window window) => _window = window;
        public void SetWorkspace(VisualStudioWorkspace workspace) => _workspace = workspace;

        private async void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => await UpdateDocumentAsync(true);

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
            SelectLine(startLinePosition);
            SelectLine(endLinePosition, true);
        }

        public void FilterBookmarks() 
            => VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel.CodeDocument, 
                filterOnBookmarks: CodeDocumentViewModel.FilterOnBookmarks, bookmarks: CodeDocumentViewModel.Bookmarks);

        public void RegionsCollapsed(RegionsCollapsedEventArgs e) => 
            OutliningHelper.RegionsCollapsed(e, CodeDocumentViewModel.CodeDocument);

        public void RegionsExpanded(RegionsExpandedEventArgs e) =>
            OutliningHelper.RegionsExpanded(e, CodeDocumentViewModel.CodeDocument);

        public void ToggleAllRegions(bool isExpanded) =>
            OutliningHelper.SetAllRegions(CodeDocumentViewModel.CodeDocument, isExpanded);

        public async Task UpdateDocumentAsync(bool forceUpdate = false)
        {
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Document activeDocument;

            try
            {
                if (Dte?.ActiveDocument != null)
                {
                    CodeDocumentViewModel.FilePath = Dte.ActiveDocument.FullName;
                }

                // Do we need to change the side where the margin is displayed
                if (_margin?.MarginSide != null &&
                    _margin?.MarginSide != Settings.Default.MarginSide &&
                    Dte != null)
                {
                    var filename = _window.Document.FullName;
                    Dte.ExecuteCommand("File.Close");
                    Dte.ExecuteCommand("File.OpenFile", filename);
                }

                activeDocument = _window.Document;
            }
            catch (ArgumentException)
            {
                // ActiveDocument is invalid, no sense to update
                return;
            }
            catch (ObjectDisposedException)
            {
                // Window/Document already disposed, no sense to update
                return;
            }
            catch (Exception e)
            {
                LogHelper.Log("Error starting UpdateDocument", e);
                return;
            }

            await Task.Run(() =>
            {
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

                    var codeItems = SyntaxMapper.MapDocument(activeDocument, this, _workspace);

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

                    // Apply current visibility settings to the document
                    VisibilityHelper.SetCodeItemVisibility(codeItems);

                    // Set the new list of codeitems as DataContext
                    CodeDocumentViewModel.CodeDocument = codeItems;
                    _cache = codeItems;

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
            });

            try
            {
                // Sync all regions
                OutliningHelper.SyncAllRegions(OutliningManager, TextView, CodeDocumentViewModel.CodeDocument);

                // Should the margin be shown and are there any items to show, if not hide the margin
                VisibilityHelper.SetMarginWidth(_column, CodeDocumentViewModel.CodeDocument);
            }
            catch (Exception e)
            {
                LogHelper.Log("Error finishing UpdateDocument", e);
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

        public void HighlightCurrentItem() => HighlightHelper.HighlightCurrentItem(_window, CodeDocumentViewModel);

        private static bool AreDocumentsEqual(List<CodeItem> existingItems, List<CodeItem> newItems)
        {
            if (existingItems == null || newItems == null) return false;
            return existingItems.SequenceEqual(newItems, new CodeItemComparer());
        }

        private void LoadBookmarksFromStorage()
        {
            if (string.IsNullOrEmpty(Dte?.Solution?.FileName)) return;

            var solutionStorage = SolutionStorageHelper.Load<SolutionStorageModel>(Dte.Solution.FileName);

            if (solutionStorage.Documents == null) return;

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

            var solutionStorage = SolutionStorageHelper.Load<SolutionStorageModel>(Dte.Solution.FileName);

            if (solutionStorage.Documents == null) return;

            var storageItem = solutionStorage.Documents
                .FirstOrDefault(s => s.FilePath.Equals(CodeDocumentViewModel.FilePath));
            if (storageItem != null)
            {
                CodeDocumentViewModel.HistoryItems = storageItem.HistoryItems;
            }
        }
    }
}
