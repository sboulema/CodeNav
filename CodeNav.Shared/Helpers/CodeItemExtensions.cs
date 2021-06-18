using CodeNav.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeNav.Helpers
{
    public static class CodeItemExtensions
    {
        public static IEnumerable<CodeItem> Flatten(this IEnumerable<CodeItem> e) 
            => e.SelectMany(c => c is IMembers ? Flatten((c as IMembers).Members) : new[] { c }).Concat(e);
    }
}
