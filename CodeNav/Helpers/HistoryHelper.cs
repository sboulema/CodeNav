using CodeNav.Models;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CodeNav.Helpers
{
    public static class HistoryHelper
    {
        public static void AddItemToHistory(CodeDocumentViewModel model, Span span)
        {
            var item = FindCodeItem(model.CodeDocument, span);
            if (item == null) return;
            model.HistoryItems.Add(item);
            ApplyHistoryIndicator(item);
        }

        public static void ApplyHistoryIndicator(CodeDocumentViewModel model)
        {
            foreach (var historyItem in model.HistoryItems)
            {
                var codeItem = model.CodeDocument
                    .Flatten()
                    .First(i => i.Id.Equals(historyItem.Id));
                ApplyHistoryIndicator(codeItem);
            }
        }

        private static void ApplyHistoryIndicator(CodeItem item)
        {
            item.StatusMoniker = KnownMonikers.History;
            item.StatusMonikerVisibility = Visibility.Visible;
        }

        private static CodeItem FindCodeItem(IEnumerable<CodeItem> items, Span span)
        {
            foreach (var item in items)
            {
                if (item.Span.Contains(span.Start) && !(item is IMembers))
                {
                    return item;
                }

                if (item is IMembers)
                {
                    return FindCodeItem(((IMembers)item).Members, span);
                }
            }

            return null;
        }
    }
}
