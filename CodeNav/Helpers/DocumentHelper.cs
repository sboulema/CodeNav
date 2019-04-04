using EnvDTE;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CodeNav.Helpers
{
    public static class DocumentHelper
    {
        /// <summary>
        /// Get the Name of the document presented in the given window
        /// </summary>
        /// <param name="window">The window containing a document</param>
        /// <returns></returns>
        public static string GetName(Window window)
        {
            var name = string.Empty;
            try
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
                name = window.Document.Name;
            }
            catch (ObjectDisposedException)
            {
                // Ignore exception we only got it trying to log
            }
            catch (Exception e)
            {
                LogHelper.Log("Error getting Name for document", e);
            }
            return name;
        }

        /// <summary>
        /// Get the FullName/FilePath for the given document
        /// </summary>
        /// <param name="document">the document</param>
        /// <returns></returns>
        public static string GetFullName(Document document)
        {
            var name = string.Empty;

            if (document == null) return name;

            try
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
                name = document.FullName;
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
            return name;
        }

        /// <summary>
        /// Get the Text for the given document
        /// </summary>
        /// <param name="document">the document</param>
        /// <returns></returns>
        public static string GetText(Document document)
        {
            var text = string.Empty;
            try
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
                var textDocument = (TextDocument)document.Object("TextDocument");
                var startPoint = textDocument?.StartPoint?.CreateEditPoint();

                if (startPoint == null)
                {
                    // Error during mapping: Unable to find TextDocument StartPoint
                    return null;
                };

                text = startPoint.GetText(textDocument.EndPoint);
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
            return text;
        }

        public static int GetNumberOfLines(Document document)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            if (document == null) return 0;

            if (File.Exists(document.FullName))
            {
                return File.ReadLines(document.FullName).Count();
            }

            return 0;
        }

        /// <summary>
        /// Get the active document
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        public static Document GetActiveDocument(DTE dte)
        {
            try
            {
                return dte?.ActiveDocument;
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
    }
}
