using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CodeNav.Helpers
{
    public static class WindowHelper
    {
        public static async Task<Window> GetWindow(DTE dte)
        {
            var filePath = await DocumentHelper.GetFilePath();

            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var windows = await GetWindows(dte);

            return windows.FirstOrDefault(window =>
            {
                return window?.Document?.FullName?.Equals(filePath, StringComparison.InvariantCultureIgnoreCase) == true;
            });
        }

        private static async Task<List<Window>> GetWindows(DTE dte)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var windowsList = new List<Window>();

            try
            {
                for (var i = 1; i < dte.Windows.Count + 1; i++)
                {
                    windowsList.Add(dte.Windows.Item(i));
                }
            }
            catch (COMException)
            {
                // Unspecified error (Exception from HRESULT: 0x80004005 (E_FAIL)) 
            }
            catch (Exception e)
            {
                LogHelper.Log("Exception getting parent window", e);
            }

            return windowsList;
        }
    }
}
