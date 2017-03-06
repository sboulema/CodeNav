using System.Collections.Generic;
using CodeNav.Models;

namespace CodeNav.Helpers
{
    public static class FindHelper
    {
        public static IEnumerable<CodeItem> Find(List<CodeItem> found, IEnumerable<CodeItem> items, int line)
        {
            foreach (var item in items)
            {
                if (item.StartLine <= line && item.EndLine >= line)
                {
                    found.Add(item);
                }

                if (item is IMembers)
                {
                    found.AddRange(Find(found, ((IMembers)item).Members, line));
                }
            }

            return found;
        }

        public static IEnumerable<CodeItem> Find(List<CodeItem> found, IEnumerable<CodeItem> items, CodeItemKindEnum kind)
        {
            foreach (var item in items)
            {
                if (item.Kind == kind)
                {
                    found.Add(item);
                }

                if (item is IMembers)
                {
                    found.AddRange(Find(found, ((IMembers)item).Members, kind));
                }
            }

            return found;
        }
    }
}
