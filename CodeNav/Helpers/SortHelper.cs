using System.Collections.Generic;
using System.Linq;
using CodeNav.Models;

namespace CodeNav.Helpers
{
    public static class SortHelper
    {
        public static List<CodeItem> SortByName(List<CodeItem> document)
        {
            document = document.OrderBy(c => c.Name).ToList();

            foreach (var item in document)
            {
                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).Members = SortByName((item as CodeClassItem).Members);
                }
                if (item is CodeNamespaceItem)
                {
                    (item as CodeNamespaceItem).Members = SortByName((item as CodeNamespaceItem).Members);
                }
            }

            return document;
        }

        public static List<CodeItem> SortByFile(List<CodeItem> document)
        {
            document = document.OrderBy(c => c.StartLine).ToList();

            foreach (var item in document)
            {
                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).Members = SortByFile((item as CodeClassItem).Members);
                }
                if (item is CodeNamespaceItem)
                {
                    (item as CodeNamespaceItem).Members = SortByFile((item as CodeNamespaceItem).Members);
                }
            }

            return document;
        }
    }
}
