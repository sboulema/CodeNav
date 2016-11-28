using System.Collections.Generic;

namespace CodeNav.Models
{
    public class CodeClassItem : CodeItem
    {
        public CodeClassItem()
        {
            Members = new List<CodeItem>();
        }

        public List<CodeItem> Members { get; set; }
    }
}
