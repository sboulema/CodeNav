using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        public static void HighlightCurrentItem(Window window, List<CodeItem> codeItems)
        {
            try
            {
                if (codeItems == null || !(window?.Selection is TextSelection)) return;
            }
            catch (Exception)
            {
                return;
            }          

            UnHighlight(codeItems);

            var itemsToHighlight = GetItemsToHighlight(codeItems, ((TextSelection)window.Selection).CurrentLine);

            //itemsToHighlight = itemsToHighlight.OrderByDescending(i => i, new CodeItemKindComparer()).ToList();

            Highlight(codeItems, itemsToHighlight.Select(i => i.Id));
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
            FrameworkElement element = null;

            // Reverse Ids, so they are in Namespace -> Class -> Method order
            //ids = ids.Reverse();

            foreach (var id in ids)
            {
                var item = FindCodeItem(document, id);
                if (item == null) return;

                item.Foreground = ToBrush(EnvironmentColors.ToolWindowTabSelectedTextColorKey);
                item.FontWeight = FontWeights.Bold;
                item.HighlightBackground = ToBrush(EnvironmentColors.AccessKeyToolTipDisabledTextColorKey);

                if (element == null)
                {
                    element = item.Control.CodeItemsControl;
                }

                var found = FindItemContainer(element as ItemsControl, item);
                if (found != null)
                {
                    element = found;

                    if (!(item is IMembers))
                    {
                        found.BringIntoView();
                    }
                }

                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).BorderBrush = ToBrush(EnvironmentColors.FileTabButtonDownSelectedActiveColorKey);
                }
            }           
        }

        private static IEnumerable<CodeItem> GetItemsToHighlight(IEnumerable<CodeItem> items, int line)
        {
            var itemsToHighlight = new List<CodeItem>();

            foreach (var item in items)
            {
                if (item.StartLine <= line && item.EndLine >= line)
                {
                    itemsToHighlight.Add(item);
                }

                if (item is IMembers)
                {
                    itemsToHighlight.AddRange(GetItemsToHighlight(((IMembers)item).Members, line));
                }
            }

            return itemsToHighlight;
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
        /// Find frameworkElement belonging to a code item
        /// </summary>
        /// <param name="itemsControl">itemsControl to search in</param>
        /// <param name="item">item to find</param>
        /// <returns></returns>
        private static FrameworkElement FindItemContainer(ItemsControl itemsControl, CodeItem item)
        {
            if (itemsControl == null) return null;

            var itemContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
            var itemContainerSubItemsControl = FindVisualChild<ItemsControl>(itemContainer);

            if (itemContainerSubItemsControl != null)
            {
                return itemContainerSubItemsControl;
            }

            if ((itemContainer as ContentPresenter)?.Content == item)
            {
                return itemContainer as FrameworkElement;
            }

            return null;
        }

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

        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }
    }

    public class CodeItemKindComparer : IComparer<CodeItem>
    {
        public int Compare(CodeItem itemA, CodeItem itemB)
        {
            switch (itemA.Kind)
            {
                case CodeItemKindEnum.Class:
                    switch (itemB.Kind)
                    {
                        case CodeItemKindEnum.Class:
                            return 0;
                        case CodeItemKindEnum.Interface:
                            return -1;
                        default:
                            return 1;
                    }
                case CodeItemKindEnum.Constant:
                case CodeItemKindEnum.Constructor:
                case CodeItemKindEnum.Delegate:
                case CodeItemKindEnum.Enum:
                case CodeItemKindEnum.EnumMember:
                case CodeItemKindEnum.Event:
                case CodeItemKindEnum.Method:
                case CodeItemKindEnum.Property:
                case CodeItemKindEnum.Variable:
                    return -1;
                case CodeItemKindEnum.Interface:
                case CodeItemKindEnum.Namespace:
                    return 1;
                case CodeItemKindEnum.Region:
                case CodeItemKindEnum.Struct:
                    switch (itemB.Kind)
                    {
                        case CodeItemKindEnum.Region:
                        case CodeItemKindEnum.Struct:
                            return 0;
                        case CodeItemKindEnum.Interface:
                        case CodeItemKindEnum.Class:
                            return -1;
                        default:
                            return 1;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
