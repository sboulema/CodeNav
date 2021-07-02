using CodeNav.Models;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CodeNav.Helpers
{
    public static class DocumentHelper
    {
        /// <summary>
        /// Get the file name/path of the active document
        /// </summary>
        /// <returns>file name/path of the active document</returns>
        public static async Task<string> GetFileName()
        {
            try
            {
                var wpfTextView = await VS.Editor.GetCurrentWpfTextViewAsync();
                return wpfTextView?.TextBuffer?.GetFileName();
            }
            catch (COMException)
            {
                // Catastrophic failure (Exception from HRESULT: 0x8000FFFF (E_UNEXPECTED))
                // We have other ways to try and map this document
            }
            catch (ArgumentException)
            {
                // The parameter is incorrect. (Exception from HRESULT: 0x80070057 (E_INVALIDARG))
                // We have other ways to try and map this document
            }
            catch (Exception e)
            {
                LogHelper.Log("Error getting file name for document", e);
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the text of the active document
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetText()
        {
            try
            {
                var wpfTextView = await VS.Editor.GetCurrentWpfTextViewAsync();
                return wpfTextView?.TextBuffer?.CurrentSnapshot?.GetText();
            }
            catch (COMException)
            {
                // Catastrophic failure (Exception from HRESULT: 0x8000FFFF (E_UNEXPECTED))
                // We were unable to get a start- or endpoint for the document. 
                // This was our last resort to map this document, nothing we can do further.
            }
            catch (Exception e)
            {
                LogHelper.Log("Error getting Text for document", e);
            }

            return string.Empty;
        }

        public static async Task<int> GetNumberOfLines()
        {
            var wpfTextView = await VS.Editor.GetCurrentWpfTextViewAsync();
            return wpfTextView?.TextViewLines?.Count ?? 0;
        }

        /// <summary>
        /// Get the active document
        /// </summary>
        /// <returns></returns>
        public static async Task<Document> GetActiveDocument()
        {
            try
            {
                var dte = await VS.GetDTEAsync();
                return dte.ActiveDocument;
            }
            catch (ArgumentException)
            {
                // ActiveDocument is invalid, no sense to update
                return null;
            }
            catch (ObjectDisposedException)
            {
                // Window/Document already disposed, no sense to update
                return null;
            }
            catch (Exception e)
            {
                LogHelper.Log("Error starting UpdateDocument", e);
                return null;
            }
        }

        public static async Task<TextSelection> GetActiveDocumentTextSelection()
        {
            var textDocument = await VS.Editor.GetActiveTextDocumentAsync();
            return textDocument?.Selection;
        }

        public static async Task<int?> GetActiveDocumentTextCurrentLine()
        {
            var textSelection = await GetActiveDocumentTextSelection();
            return textSelection?.CurrentLine;
        }
    }
}
