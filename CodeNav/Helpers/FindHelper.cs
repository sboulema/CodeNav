using CodeNav.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeNav.Helpers
{
    public static class FindHelper
    {
        public static CodeItem FindCodeItem(IEnumerable<CodeItem> items, string id)
        {
            if (items == null || !items.Any() || string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (var item in items)
            {
                if (item.Id.Equals(id))
                {
                    return item;
                }

                if (item is IMembers hasMembersItem && hasMembersItem.Members.Any())
                {
                    var found = FindCodeItem(hasMembersItem.Members, id);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }
    }
}
