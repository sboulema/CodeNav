using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Models;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Window = EnvDTE.Window;

namespace CodeNav.Helpers
{
    public static class HighlightHelper
    {
        public static void HighlightCurrentItem(Window window, CodeDocumentViewModel codeDocumentViewModel)
        {
            if (Settings.Default.DisableHighlight) return;

            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            try
            {
                if (!(window?.Selection is TextSelection)) return;
            }
            catch (Exception)
            {
                return;
            }

            HighlightCurrentItem(codeDocumentViewModel, ((TextSelection)window.Selection).CurrentLine,
                ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTabSelectedTextColorKey),
                GetBackgroundBrush().Color,
                ColorHelper.ToMediaColor(EnvironmentColors.FileTabButtonDownSelectedActiveColorKey),
                ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTextColorKey));
        }

        public static void HighlightCurrentItem(CodeDocumentViewModel codeDocumentViewModel, int currentLine, 
            Color foregroundColor, Color backgroundColor, Color borderColor, Color regularForegroundColor)
        {
            if (codeDocumentViewModel == null) return;

            UnHighlight(codeDocumentViewModel, regularForegroundColor);
            var itemsToHighlight = GetItemsToHighlight(codeDocumentViewModel.CodeDocument, currentLine);
            Highlight(codeDocumentViewModel, itemsToHighlight.Select(i => i.Id), foregroundColor, backgroundColor, borderColor);
        }

        private static void UnHighlight(CodeDocumentViewModel codeDocumentViewModel, Color foregroundColor) =>
            UnHighlight(codeDocumentViewModel.CodeDocument, foregroundColor, codeDocumentViewModel.Bookmarks);

        private static void UnHighlight(List<CodeItem> codeItems, Color foregroundColor, 
            Dictionary<string, int> bookmarks)
        {
            foreach (var item in codeItems)
            {
                if (item == null) continue;
               
                item.FontWeight = FontWeights.Regular;
                item.NameBackgroundColor = Brushes.Transparent.Color;
                item.IsHighlighted = false;

                if (!BookmarkHelper.IsBookmark(bookmarks, item))
                {
                    item.ForegroundColor = foregroundColor;
                } else
                {
                    item.ForegroundColor = item.BookmarkStyles[bookmarks[item.Id]].ForegroundColor;
                }

                if (item is IMembers)
                {
                    var hasMembersItem = (IMembers)item;

                    if (hasMembersItem.Members.Any())
                    {
                        UnHighlight(hasMembersItem.Members, foregroundColor, bookmarks);
                    }
                }

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    classItem.BorderColor = Colors.DarkGray;
                }
            }
        }

        /// <summary>
        /// Given a list of unique ids and a code document, find all code items and 'highlight' them.
        /// Highlighting changes the foreground, fontweight and background of a code item
        /// </summary>
        /// <param name="document">Code document</param>
        /// <param name="ids">List of unique code item ids</param>
        private static void Highlight(CodeDocumentViewModel codeDocumentViewModel, IEnumerable<string> ids, 
            Color foregroundColor, Color backgroundColor, Color borderColor)
        {
            FrameworkElement element = null;

            foreach (var id in ids)
            {
                var item = FindCodeItem(codeDocumentViewModel.CodeDocument, id);
                if (item == null) return;

                item.ForegroundColor = foregroundColor;
                item.FontWeight = FontWeights.Bold;
                item.NameBackgroundColor = backgroundColor;
                item.IsHighlighted = true;

                if (element == null && item.Control != null)
                {
                    element = GetCodeItemsControl(item.Control);
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
                    (item as CodeClassItem).BorderColor = borderColor;
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
                item.ForegroundColor = ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTextColorKey);

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

        private static SolidColorBrush GetBackgroundBrush()
        {
            var highlightBackgroundColor = Settings.Default.HighlightBackgroundColor;
            if (highlightBackgroundColor.IsNamedColor && highlightBackgroundColor.Name.Equals("Transparent"))
            {
                return ColorHelper.ToBrush(EnvironmentColors.AccessKeyToolTipDisabledTextColorKey);
            }

            return ColorHelper.ToBrush(highlightBackgroundColor);
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

        private static FrameworkElement GetCodeItemsControl(ICodeViewUserControl control)
        {
            if (control is CodeViewUserControl)
            {
                return (control as CodeViewUserControl).CodeItemsControl;
            }

            return (control as CodeViewUserControlTop).CodeItemsControl;
        }

        public static void SetSelectedIndex(List<CodeItem> items)
        {
            foreach (CodeDepthGroupItem groupItem in items)
            {
                var selectedItem = groupItem.Members.LastOrDefault(i => i.IsHighlighted);
                groupItem.SelectedIndex = groupItem.Members.IndexOf(selectedItem);
            }
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
