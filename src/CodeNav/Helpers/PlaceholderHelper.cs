using CodeNav.ViewModels;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.Helpers;

public static class PlaceholderHelper
{
    public static IEnumerable<CodeItem> CreateLoadingItem()
        => CreateItem("Loading...", ImageMoniker.KnownValues.Refresh);

    public static IEnumerable<CodeItem> CreateSelectDocumentItem()
        => CreateItem("Waiting for active code document...", ImageMoniker.KnownValues.DocumentOutline);

    public static IEnumerable<CodeItem> CreateLineThresholdPassedItem()
        => CreateItem("File exceeds line threshold...", ImageMoniker.KnownValues.DocumentError);

    private static IEnumerable<CodeItem> CreateItem(string name, ImageMoniker moniker)
        => [
            new CodeItem
            {
                Name = name,
                FullName = name,
                Id = name,
                Tooltip = name,
                Moniker = moniker,
            }
        ];
}
