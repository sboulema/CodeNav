using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;

namespace CodeNav.Helpers
{
    public static class ProjectHelper
    {
        static ProjectHelper()
        {
            DTE = (DTE2)Package.GetGlobalService(typeof(DTE));
        }

        public static DTE2 DTE { get; }

        public static async Task<string> GetSolutionFileName()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            return DTE?.Solution?.FileName;
        }
    }
}
