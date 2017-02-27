using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CodeNav.Mappers;
using CodeNav.Models;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Color = System.Windows.Media.Color;
using Window = EnvDTE.Window;

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
                item.FontWeight = FontWeights.Regular;

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

        /// <summary>
        /// Given a list of unique ids and a code document, find all code items and 'highlight' them.
        /// Highlighting changes the foreground, fontweight and background of a code item
        /// </summary>
        /// <param name="document">Code document</param>
        /// <param name="ids">List of unique code item ids</param>
        private static void Highlight(List<CodeItem> document, IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                var item = FindCodeItem(document, id);
                if (item == null) return;

                item.Foreground = ToBrush(EnvironmentColors.ToolWindowTabSelectedTextColorKey);
                item.FontWeight = FontWeights.Bold;
                item.HighlightBackground = ToBrush(EnvironmentColors.AccessKeyToolTipDisabledTextColorKey);

                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).BorderBrush = ToBrush(EnvironmentColors.FileTabButtonDownSelectedActiveColorKey);
                }
            }
        }

        /// <summary>
        /// Get list of unique item ids that should be highlighted
        /// Given a code element will find parent code elements that should also be highlighted
        /// </summary>
        /// <param name="list">Code document</param>
        /// <param name="element">Code element that should be highlighted</param>
        private static void GetItemsToHighlight(ICollection<string> list, CodeElement element)
        {
            list.Add(SyntaxMapper.MapId(element));      

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

        /// <summary>
        /// Convert from VSTheme EnvironmentColor to a XAML SolidColorBrush
        /// </summary>
        /// <param name="key">VSTheme EnvironmentColor key</param>
        /// <returns>XAML SolidColorBrush</returns>
        private static SolidColorBrush ToBrush(ThemeResourceKey key)
        {
            var color = VSColorTheme.GetThemedColor(key);
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        /// <summary>
        /// Find particular code item by its id inside of a code document
        /// </summary>
        /// <param name="items">Code document</param>
        /// <param name="id">unqiue item id</param>
        /// <returns></returns>
        private static CodeItem FindCodeItem(IEnumerable<CodeItem> items, string id)
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
