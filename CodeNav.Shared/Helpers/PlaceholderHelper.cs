using CodeNav.Models;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using System.Collections.Generic;
using System.Windows.Media;

namespace CodeNav.Helpers
{
    public static class PlaceholderHelper
    {
        public static List<CodeItem> CreateLoadingItem()
            => CreateItem("Loading...", KnownMonikers.Refresh);

        public static List<CodeItem> CreateSelectDocumentItem()
            => CreateItem("Waiting for active code document...", KnownMonikers.DocumentOutline);

        public static List<CodeItem> CreateLineThresholdPassedItem()
            => CreateItem("Click Refresh to load file...", KnownMonikers.DocumentError);

        private static List<CodeItem> CreateItem(string name, ImageMoniker moniker)
        {
            return new List<CodeItem>
            {
                new CodeNamespaceItem
                {
                    Id = name,
                    Members = new List<CodeItem>
                    {
                        new CodeClassItem
                        {
                            Name = name,
                            FullName = name,
                            Id = name,
                            ForegroundColor = Colors.Black,
                            BorderColor = Colors.DarkGray,
                            Moniker = moniker
                        }
                    }
                }
            };
        }
    }
}
