using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CodeNav.Helpers
{
    public static class DocumentHelper
    {
        /// <summary>
        /// Get the Name of the document presented in the given window
        /// </summary>
        /// <param name="window">The window containing a document</param>
        /// <returns></returns>
        public static async Task<string> GetName(Window window)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                return window.Document.Name;
            }
            catch (ObjectDisposedException)
            {
                // Ignore exception we only got it trying to log
            }
            catch (Exception e)
            {
                LogHelper.Log("Error getting Name for document", e);
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the FullName/FilePath for the given document
        /// </summary>
        /// <param name="document">the document</param>
        /// <returns></returns>
        public static async Task<string> GetFullName(Document document)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                return document?.FullName;
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
                LogHelper.Log("Error getting FullName for document", e);
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the Text for the given document
        /// </summary>
        /// <param name="document">the document</param>
        /// <returns></returns>
        public static async Task<string> GetText(Document document)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var textDocument = (TextDocument)document.Object("TextDocument");
                var startPoint = textDocument?.StartPoint?.CreateEditPoint();

                if (startPoint == null)
                {
                    // Error during mapping: Unable to find TextDocument StartPoint
                    return null;
                };

                return startPoint.GetText(textDocument.EndPoint);
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
            var document = GetActiveDocument();

            if (document == null)
            {
                return 0;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var fullName = document.FullName;

            if (!File.Exists(fullName))
            {
                return 0;
            }

            return File.ReadLines(fullName).Count();
        }

        /// <summary>
        /// Get the active document
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        public static Document GetActiveDocument()
        {
            try
            {
                return ProjectHelper.DTE.ActiveDocument;
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
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var document = GetActiveDocument();

            try
            {
                return document?.Selection as TextSelection;
            }
            catch
            {
            }

            return null;
        }

        public static async Task<int?> GetActiveDocumentTextCurrentLine()
        {
            var textSelection = await GetActiveDocumentTextSelection();
            return textSelection?.CurrentLine;
        }
    }
}
