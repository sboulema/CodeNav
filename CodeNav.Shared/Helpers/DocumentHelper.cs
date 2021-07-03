using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Shapes;

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

        public static async Task SelectLines(LinePosition start, LinePosition end)
        {
            try
            {
                var documentView = await VS.Documents.GetActiveDocumentViewAsync();

                var span = new Span(start.Character, end.Character - start.Character);
                var snapShotSpan = new SnapshotSpan(documentView?.TextBuffer.CurrentSnapshot, span);

                documentView.TextView.Selection.Select(snapShotSpan, false);
                documentView?.TextView?.ViewScroller.EnsureSpanVisible(snapShotSpan);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        public static async Task<Document> GetCodeAnalysisDocument(VisualStudioWorkspace workspace)
        {
            var filePath = await GetFilePath();

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
    }
}
