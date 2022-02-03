using CodeNav.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeNav.Extensions
{
    public static class CodeItemExtensions
    {
        /// <summary>
        /// Transform a nested list of CodeItems to a flat list
        /// </summary>
        /// <param name="codeDocument">Nested list of CodeItems</param>
        /// <returns>Flat list of CodeItems</returns>
        public static IEnumerable<CodeItem> Flatten(this IEnumerable<CodeItem> codeDocument) 
            => codeDocument
                .SelectMany(codeItem => codeItem is IMembers codeMembersItem
                    ? Flatten(codeMembersItem.Members) : new[] { codeItem }).Concat(codeDocument);

        public static IEnumerable<CodeItem> FilterNull(this IEnumerable<CodeItem> codeDocument)
            => codeDocument.Where(codeItem => codeItem != null);
    }
}
