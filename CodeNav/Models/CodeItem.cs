using EnvDTE;

namespace CodeNav.Models
{
    public class CodeItem
    {
        public string Name { get; set; }
        public TextPoint StartPoint { get; set; }
        public string IconPath { get; set; }
    }
}
