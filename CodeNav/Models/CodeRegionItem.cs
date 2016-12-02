using EnvDTE;

namespace CodeNav.Models
{
    public class CodeRegionItem : CodeClassItem
    {
        public TextPoint EndPoint { get; set; }
    }
}
