using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CodeNav.Models;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Color = System.Windows.Media.Color;

namespace CodeNav.Helpers
{
    public static class HighlightHelper
    {
        public static void UnHighlight(List<CodeItem> document, List<string> itemNames)
        {
            foreach (var name in itemNames)
            {
                var item = FindCodeItem(document, name);
                if (item == null) return;

                item.Foreground = ToBrush(EnvironmentColors.ToolWindowTextColorKey);

                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).BorderBrush = new SolidColorBrush(Colors.DarkGray);
                }
            }
        }

        public static void Highlight(List<CodeItem> document, List<string> itemNames)
        {
            foreach (var name in itemNames)
            {
                var item = FindCodeItem(document, name);
                if (item == null) return;

                item.Foreground = new SolidColorBrush(Colors.SteelBlue);

                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).BorderBrush = new SolidColorBrush(Colors.SteelBlue);
                }
            }
        }

        public static void GetItemsToHighlight(List<string> list, CodeElement element)
        {
            list.Add(element.FullName);

            var parent = element.Collection.Parent;
            if (parent == null || parent is CodeElement == false) return;

            GetItemsToHighlight(list, parent);
        }

        public static void SetForeground(IEnumerable<CodeItem> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                item.Foreground = ToBrush(EnvironmentColors.ToolWindowTextColorKey);

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem) item;
                    if (classItem.Members.Any())
                    {
                        SetForeground(classItem.Members);
                    }
                }
            }
        }

        private static SolidColorBrush ToBrush(ThemeResourceKey key)
        {
            var color = VSColorTheme.GetThemedColor(key);
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private static CodeItem FindCodeItem(List<CodeItem> items, string itemFullName)
        {
            foreach (var item in items)
            {
                if (item.FullName.Equals(itemFullName))
                {
                    return item;
                }

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    if (classItem.Members.Any())
                    {
                        var found = FindCodeItem(classItem.Members, itemFullName);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
            }
            return null;
        }
    }
}
