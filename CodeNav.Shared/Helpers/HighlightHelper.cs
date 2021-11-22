using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CodeNav.Helpers
{
    public static class HighlightHelper
    {
        public static void HighlightCurrentItem(CodeDocumentViewModel codeDocumentViewModel,
            Color backgroundBrushColor, int lineNumber)
        {
            if (codeDocumentViewModel == null)
            {
                return;
            }

            HighlightCurrentItem(codeDocumentViewModel, lineNumber,
                ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTabSelectedTextColorKey),
                backgroundBrushColor,
                ColorHelper.ToMediaColor(EnvironmentColors.FileTabButtonDownSelectedActiveColorKey),
                ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTextColorKey))
            .FireAndForget();
        }

        public static async Task HighlightCurrentItem(CodeDocumentViewModel codeDocumentViewModel, int lineNumber,
            Color foregroundColor, Color backgroundColor, Color borderColor, Color regularForegroundColor)
        {
            var bookmarkStyles = await BookmarkHelper.GetBookmarkStyles(codeDocumentViewModel);

            UnHighlight(codeDocumentViewModel.CodeDocument, regularForegroundColor, codeDocumentViewModel.Bookmarks, bookmarkStyles);
            var itemsToHighlight = GetItemsToHighlight(codeDocumentViewModel.CodeDocument, lineNumber);
            await Highlight(codeDocumentViewModel, itemsToHighlight.Select(i => i.Id), foregroundColor, backgroundColor, borderColor);
        }

        private static void UnHighlight(List<CodeItem> codeItems, Color foregroundColor, 
            Dictionary<string, int> bookmarks, List<BookmarkStyle> bookmarkStyles)
        {
            foreach (var item in codeItems)
            {
                if (item == null)
                {
                    return;
                }

                item.FontWeight = FontWeights.Regular;
                item.NameBackgroundColor = Brushes.Transparent.Color;
                item.IsHighlighted = false;

                if (!BookmarkHelper.IsBookmark(bookmarks, item))
                {
                    item.ForegroundColor = foregroundColor;
                }
                else
                {
                    item.ForegroundColor = bookmarkStyles[bookmarks[item.Id]].ForegroundColor;
                }

                if (item is IMembers hasMembersItem && hasMembersItem.Members.Any())
                {
                    UnHighlight(hasMembersItem.Members, foregroundColor, bookmarks, bookmarkStyles);
                }

                if (item is CodeClassItem classItem)
                {
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
        private static async Task Highlight(CodeDocumentViewModel codeDocumentViewModel, IEnumerable<string> ids, 
            Color foregroundColor, Color backgroundColor, Color borderColor)
        {
            FrameworkElement element = null;

            var tasks = ids.Select(async id => {
                var item = FindCodeItem(codeDocumentViewModel.CodeDocument, id);

                if (item == null)
                {
                    return;
                }

                item.ForegroundColor = foregroundColor;
                item.FontWeight = FontWeights.Bold;
                item.NameBackgroundColor = backgroundColor;
                item.IsHighlighted = true;

                if (element == null && item.Control != null)
                {
                    element = GetCodeItemsControl(item.Control);
                }

                var found = await FindItemContainer(element as ItemsControl, item);
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
            });

            await Task.WhenAll(tasks);
        }

        private static IEnumerable<CodeItem> GetItemsToHighlight(IEnumerable<CodeItem> items, int line)
        {
            var itemsToHighlight = new List<CodeItem>();

            Parallel.ForEach(items, item => 
            {
                if (item.StartLine <= line && item.EndLine >= line)
                {
                    itemsToHighlight.Add(item);
                }

                if (item is IMembers hasMembersItem)
                {
                    itemsToHighlight.AddRange(GetItemsToHighlight(hasMembersItem.Members, line));
                }
            });

            return itemsToHighlight;
        }
            
        public static void SetForeground(IEnumerable<CodeItem> items)
        {
            if (items == null)
            {
                return;
            }

            Parallel.ForEach(items, item => 
            {
                item.ForegroundColor = ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTextColorKey);

                if (item is IMembers hasMembersItem && hasMembersItem.Members.Any())
                {
                    SetForeground(hasMembersItem.Members);
                }
            });
        }

        public static async Task<Color> GetBackgroundHighlightColor()
        {
            var general = await General.GetLiveInstanceAsync();

            var highlightBackgroundColor = general.HighlightColor;

            if (highlightBackgroundColor.IsNamedColor &&
                highlightBackgroundColor.Name.Equals("Transparent"))
            {
                return ColorHelper.ToMediaColor(EnvironmentColors.SystemHighlightColorKey);
            }

            return ColorHelper.ToMediaColor(highlightBackgroundColor);
        }

        /// <summary>
        /// Find frameworkElement belonging to a code item
        /// </summary>
        /// <param name="itemsControl">itemsControl to search in</param>
        /// <param name="item">item to find</param>
        /// <returns></returns>
        private static async Task<FrameworkElement> FindItemContainer(ItemsControl itemsControl, CodeItem item)
        {
            if (itemsControl == null)
            {
                return null;
            }

            var itemContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
            var itemContainerSubItemsControl = await FindVisualChild<ItemsControl>(itemContainer);

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

                if (item is IMembers hasMembersItem && hasMembersItem.Members.Any())
                {
                    var found = FindCodeItem(hasMembersItem.Members, id);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        public static async Task<T> FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T t)
                {
                    return t;
                }

                var childItem = await FindVisualChild<T>(child);
                if (childItem != null)
                {
                    return childItem;
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
            Parallel.ForEach(items, item => 
            {
                var groupItem = item as CodeDepthGroupItem;
                var selectedItem = groupItem.Members.LastOrDefault(i => i.IsHighlighted);
                groupItem.SelectedIndex = selectedItem != null ? groupItem.Members.IndexOf(selectedItem) : 0;
            });
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
