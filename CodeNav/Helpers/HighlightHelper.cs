using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using CodeNav.Mappers;
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
                if (item == null) continue;

                item.Foreground = ToBrush(EnvironmentColors.ToolWindowTextColorKey);

                if (item is IMembers)
                {
                    var hasMembersItem = (IMembers)item;                 
                    
                    if (hasMembersItem.Members.Any())
                    {
                        UnHighlight(hasMembersItem.Members);
                    }
                }

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    classItem.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                }
            }
        }

        private static void Highlight(List<CodeItem> document, List<string> ids)
        {
            foreach (var id in ids)
            {
                var item = FindCodeItem(document, id);
                if (item == null) return;

                item.Foreground = new SolidColorBrush(Colors.SteelBlue);

                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).BorderBrush = new SolidColorBrush(Colors.SteelBlue);
                }
            }
        }

        private static void GetItemsToHighlight(ICollection<string> list, CodeElement element)
        {
            list.Add(CodeItemMapper.MapId(element));      

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

                if (item is IMembers)
                {
                    var hasMembersItem = (IMembers) item;
                    if (hasMembersItem.Members.Any())
                    {
                        SetForeground(hasMembersItem.Members);
                    }
                }
            }
        }

        private static SolidColorBrush ToBrush(ThemeResourceKey key)
        {
            var color = VSColorTheme.GetThemedColor(key);
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private static CodeItem FindCodeItem(List<CodeItem> items, string id)
        {
            foreach (var item in items)
            {
                if (item.Id.Equals(id))
                {
                    return item;
                }

                if (item is IMembers)
                {
                    var hasMembersItem = (IMembers)item;
                    if (hasMembersItem.Members.Any())
                    {
                        var found = FindCodeItem(hasMembersItem.Members, id);
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
