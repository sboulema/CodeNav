using System;
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
        public static List<CodeItem> HighlightCurrentItem(Window window, List<CodeItem> codeItems)
        {
            try
            {
                if (codeItems == null || window?.Selection == null) return codeItems;
            }
            catch (Exception)
            {
                return codeItems;
            }          

            var textSelection = window.Selection as TextSelection;

            var currentFunctionElement = textSelection?.ActivePoint.CodeElement[vsCMElement.vsCMElementFunction];

            if (currentFunctionElement == null)
            {
                UnHighlight(codeItems);
                return codeItems;
            }

            UnHighlight(codeItems);

            var highlightedItems = new List<string>();
            GetItemsToHighlight(highlightedItems, currentFunctionElement);

            Highlight(codeItems, highlightedItems);

            return codeItems;
        }

        private static void UnHighlight(List<CodeItem> document)
        {
            foreach (var item in document)
            {
                item.Foreground = ToBrush(EnvironmentColors.ToolWindowTextColorKey);

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    classItem.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                    
                    if (classItem.Members.Any())
                    {
                        UnHighlight(classItem.Members);
                    }
                }

                if (item is CodeNamespaceItem)
                {
                    var nsItem = (CodeNamespaceItem)item;
                    if (nsItem.Members.Any())
                    {
                        UnHighlight(nsItem.Members);
                    }
                }
            }
        }

        private static void Highlight(List<CodeItem> document, List<string> itemNames)
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

        private static void GetItemsToHighlight(List<string> list, CodeElement element)
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

                if (item is CodeNamespaceItem)
                {
                    var nsItem = (CodeNamespaceItem)item;
                    if (nsItem.Members.Any())
                    {
                        SetForeground(nsItem.Members);
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

                if (item is CodeNamespaceItem)
                {
                    var nsItem = (CodeNamespaceItem)item;
                    if (nsItem.Members.Any())
                    {
                        var found = FindCodeItem(nsItem.Members, itemFullName);
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
