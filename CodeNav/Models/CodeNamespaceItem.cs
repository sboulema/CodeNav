using System.Collections.Generic;

namespace CodeNav.Models
{
    public class CodeNamespaceItem : CodeItem, IMembers
    {
        public CodeNamespaceItem()
        {
            Members = new List<CodeItem>();
        }

        public List<CodeItem> Members { get; set; }
    }
}
