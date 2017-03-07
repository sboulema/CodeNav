using EnvDTE;

namespace CodeNav.Models
{
    public class BackgroundWorkerRequest
    {
        public Document Document;
        public string SolutionFilePath;
        public bool ForceUpdate;
    }
}
