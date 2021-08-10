using CodeNav.Mappers;
using CodeNav.Models;
using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Settings = CodeNav.Properties.Settings;
using Task = System.Threading.Tasks.Task;

namespace CodeNav.Helpers
{
    public static class DocumentHelper
    {
        public static async Task<string> GetFilePath()
        {
            try
            {
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();
                return documentView?.Document?.FilePath;
            }
            catch (Exception)
            {
                // Ignore
            }

            return string.Empty;
        }

        public static async Task<string> GetText()
        {
            try
            {
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();
                return documentView?.TextBuffer?.CurrentSnapshot?.GetText();
            }
            catch (Exception)
            {
                // Ignore
            }

            return string.Empty;
        }

        public static async Task<int> GetNumberOfLines()
        {
            try
            {
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();
                return documentView?.TextView?.TextViewLines?.Count ?? 0;
            }
            catch (Exception)
            {
                // Ignore
            }

            return 0;
        }

        public static async Task<int?> GetCurrentLineNumber()
        {
            try
            {
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();
                return documentView?.TextView?.Selection.ActivePoint.Position.GetContainingLine().LineNumber;
            }
            catch (Exception)
            {
                // Ignore
            }

            return null;
        }

        public static async Task ScrollToLine(LinePosition linePosition)
        {
            try
            {
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();
                var line = documentView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(linePosition.Line);
                documentView?.TextView?.ViewScroller.EnsureSpanVisible(line.Extent);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public static async Task SelectLines(TextSpan textSpan)
        {
            try
            {
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();

                var span = new Span(textSpan.Start, textSpan.Length);
                var snapShotSpan = new SnapshotSpan(documentView?.TextBuffer.CurrentSnapshot, span);

                documentView?.TextView?.Selection.Select(snapShotSpan, false);
                documentView?.TextView?.ViewScroller.EnsureSpanVisible(snapShotSpan, EnsureSpanVisibleOptions.AlwaysCenter);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public static async Task<Document> GetCodeAnalysisDocument(VisualStudioWorkspace workspace, string filePath = "")
        {
            if (workspace == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = await GetFilePath();
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var documentId = workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath).FirstOrDefault();

            if (documentId == null)
            {
                return null;
            }

            return workspace.CurrentSolution.GetDocument(documentId);
        }

        public static async Task<bool> IsLargeDocument()
        {
            if (Settings.Default.AutoLoadLineThreshold == 0)
            {
                return false;
            }

            return await GetNumberOfLines() > Settings.Default.AutoLoadLineThreshold;
        }

        public static async Task UpdateDocument(ICodeViewUserControl control,
            VisualStudioWorkspace workspace,
            CodeDocumentViewModel codeDocumentViewModel,
            IOutliningManagerService outliningManagerService,
            CodeNavMargin margin,
            ColumnDefinition column = null,
            RowDefinition row = null,
            string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = await GetFilePath();
            }

            codeDocumentViewModel.FilePath = filePath;

            try
            {
                // If not show a loading item
                if (!codeDocumentViewModel.CodeDocument.Any())
                {
                    codeDocumentViewModel.CodeDocument = PlaceholderHelper.CreateLoadingItem();
                }

                var codeItems = await SyntaxMapper.MapDocument(control, workspace, filePath);

                if (codeItems == null)
                {
                    // CodeNav for document updated, no results
                    return;
                }

                // Filter all null items from the code document
                SyntaxMapper.FilterNullItems(codeItems);

                // Sort items
                codeDocumentViewModel.SortOrder = Settings.Default.SortOrder;
                SortHelper.Sort(codeItems, Settings.Default.SortOrder);

                // Set currently active codeitem
                HighlightHelper.SetForeground(codeItems);

                // Set the new list of codeitems as DataContext
                codeDocumentViewModel.CodeDocument = codeItems;

                // Apply current visibility settings to the document
                VisibilityHelper.SetCodeItemVisibility(codeDocumentViewModel);

                // Apply bookmarks
                codeDocumentViewModel.Bookmarks = await BookmarkHelper.LoadBookmarksFromStorage(codeDocumentViewModel.FilePath);
                BookmarkHelper.ApplyBookmarks(codeDocumentViewModel).FireAndForget();

                // Apply history items
                codeDocumentViewModel.HistoryItems = await HistoryHelper.LoadHistoryItemsFromStorage(codeDocumentViewModel.FilePath);
                HistoryHelper.ApplyHistoryIndicator(codeDocumentViewModel);
            }
            catch (Exception e)
            {
                LogHelper.Log("Error running UpdateDocument", e);
            }

            try
            {
                // Sync all regions
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();
                OutliningHelper.SyncAllRegions(outliningManagerService, documentView?.TextView, codeDocumentViewModel.CodeDocument);

                // Should the margin be shown and are there any items to show, if not hide the margin
                if (column != null)
                {
                    VisibilityHelper.SetMarginWidth(column, codeDocumentViewModel.CodeDocument).FireAndForget();
                }
                else if (row != null)
                {
                    VisibilityHelper.SetMarginHeight(row, codeDocumentViewModel.CodeDocument).FireAndForget();
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("Error finishing UpdateDocument", e);
            }
        }
    }
}
