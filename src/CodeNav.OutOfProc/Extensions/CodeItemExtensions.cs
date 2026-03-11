using CodeNav.OutOfProc.Interfaces;
using CodeNav.OutOfProc.ViewModels;

namespace CodeNav.OutOfProc.Extensions;

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
                ? Flatten(codeMembersItem.Members)
                : [codeItem])
            .Concat(codeDocument);

    /// <summary>
    /// Delete null items from a flat list of CodeItems
    /// </summary>
    /// <param name="codeDocument">Flat list of CodeItems</param>
    /// <returns>Flat list of CodeItems</returns>
    public static IEnumerable<CodeItem> FilterNull(this IEnumerable<CodeItem> codeDocument)
        => codeDocument.Where(codeItem => codeItem != null);

    public static void AddIfNotNull(this List<CodeItem> items, CodeItem? item)
    {
        if (item == null)
        {
            return;
        }

        items.Add(item);
    }
}
