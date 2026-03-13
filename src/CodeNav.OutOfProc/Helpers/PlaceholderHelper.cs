using CodeNav.OutOfProc.ViewModels;
using Microsoft.VisualStudio.Extensibility;

namespace CodeNav.OutOfProc.Helpers;

public static class PlaceholderHelper
{
    public static List<CodeItem> CreateLoadingItem()
        => CreateItem("Loading...", ImageMoniker.KnownValues.Refresh);

    public static List<CodeItem> CreateSelectDocumentItem()
        => CreateItem("Waiting for active code document...", ImageMoniker.KnownValues.DocumentOutline);

    public static List<CodeItem> CreateLineThresholdPassedItem()
        => CreateItem("File exceeds line threshold...", ImageMoniker.KnownValues.DocumentError);

    public static List<CodeItem> CreateNoCodeItemsFound()
        => CreateItem("No code items found...", ImageMoniker.KnownValues.DocumentOutline);

    private static List<CodeItem> CreateItem(string name, ImageMoniker moniker)
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
