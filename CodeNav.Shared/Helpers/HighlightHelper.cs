using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Extensions;
using CodeNav.Models;
using CodeNav.Models.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CodeNav.Helpers
{
    public static class HighlightHelper
    {
        private static Color _foregroundColor = ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTabSelectedTextColorKey);
        private static Color _borderColor = ColorHelper.ToMediaColor(EnvironmentColors.FileTabButtonDownSelectedActiveColorKey);
        private static Color _regularForegroundColor = ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTextColorKey);

        /// <summary>
        /// Highlight code items that contain the current line number
        /// </summary>
        /// <param name="codeDocumentViewModel">Code document</param>
        /// <param name="backgroundColor">Background color to use when highlighting</param>
        /// <param name="lineNumber">Current line number</param>
        public static void HighlightCurrentItem(CodeDocumentViewModel codeDocumentViewModel,
            Color backgroundColor, int lineNumber)
        {
            if (codeDocumentViewModel == null)
            {
                return;
            }

            try
            {
                UnHighlight(codeDocumentViewModel);
                Highlight(codeDocumentViewModel, lineNumber, backgroundColor).FireAndForget();
            }
            catch (Exception e)
            {
                LogHelper.Log("Error highlighting current item", e);
            }
        }

        /// <summary>
        /// Remove highlight from all code items
        /// </summary>
        /// <remarks>Will restore bookmark foreground color when unhighlighting a bookmarked item</remarks>
        /// <param name="codeDocumentViewModel">Code document</param>
        public static void UnHighlight(CodeDocumentViewModel codeDocumentViewModel)
            => codeDocumentViewModel.CodeDocument
                .Flatten()
                .FilterNull()
                .ToList()
                .ForEach(item =>
                {
                    item.FontWeight = FontWeights.Regular;
                    item.NameBackgroundColor = Brushes.Transparent.Color;
                    item.IsHighlighted = false;
                    item.ForegroundColor = BookmarkHelper.IsBookmark(codeDocumentViewModel.Bookmarks, item)
                        ? codeDocumentViewModel.BookmarkStyles[codeDocumentViewModel.Bookmarks[item.Id]].ForegroundColor
                        : _regularForegroundColor;

                    if (item is CodeClassItem classItem)
                    {
                        classItem.BorderColor = Colors.DarkGray;
                    }
                });

        /// <summary>
        /// Highlight code items that contain the current line number
        /// </summary>
        /// <remarks>
        /// Highlighting changes the foreground, fontweight and background of a code item
        /// Deepest highlighted code item will be scrolled to, to ensure it is in view
        /// </remarks>
        /// <param name="document">Code document</param>
        /// <param name="ids">List of unique code item ids</param>
        private static async Task Highlight(CodeDocumentViewModel codeDocumentViewModel,
            int lineNumber, Color backgroundColor)
        {
            FrameworkElement frameworkElement = null;

            var itemsToHighlight = codeDocumentViewModel
                .CodeDocument
                .Flatten()
                .FilterNull()
                .Where(item => item.StartLine <= lineNumber && item.EndLine >= lineNumber)
                .OrderBy(item => item.StartLine);

            foreach (var item in itemsToHighlight)
            {
                item.ForegroundColor = _foregroundColor;
                item.FontWeight = FontWeights.Bold;
                item.NameBackgroundColor = backgroundColor;
                item.IsHighlighted = true;

                if (item is CodeClassItem classItem)
                {
                    classItem.BorderColor = _borderColor;
                }

                frameworkElement = await FindItemContainer(item, frameworkElement, codeDocumentViewModel);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            frameworkElement?.BringIntoView();

            //var smallestCodeItem = itemsToHighlight
            //    .OrderBy(item => (lineNumber - item?.StartLine) + (item?.EndLine - lineNumber))
            //    .FirstOrDefault();

            //var codeItem = FindCodeItem(codeDocumentViewModel.CodeDocument, smallestCodeItem);

            //await BringIntoView(smallestCodeItem);

            //await BringIntoView(codeItem);
        }

        /// <summary>
        /// Find frameworkElement belonging to a code item
        /// </summary>
        /// <remarks>
        /// Must be called recursively to gradually move the frameworkElement close to the highlighted code item
        /// </remarks>
        /// <param name="frameworkElement">FrameworkElement to get the item container from</param>
        /// <param name="item">Code item of which whe want to find the container</param>
        /// <returns></returns>
        private static async Task<FrameworkElement> FindItemContainer(CodeItem item,
            FrameworkElement frameworkElement, CodeDocumentViewModel codeDocumentViewModel)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (item.Control == null)
            {
                return null;
            }

            if (frameworkElement == null)
            {
                frameworkElement = GetCodeItemsControl(item.Control);
            }

            if (frameworkElement == null)
            {
                return null;
            }

            var itemContainer = (frameworkElement as ItemsControl).ItemContainerGenerator.ContainerFromItem(item);
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

        /// <summary>
        /// Based on a code item find the same code item within the view model
        /// </summary>
        /// <remarks>
        /// We need to find the exact code item in the view model nested list,
        /// else we cannot find its WPF UI ItemContainer
        /// </remarks>
        /// <param name="codeItems">List of code items</param>
        /// <param name="codeItem">code item to be found</param>
        /// <returns></returns>
        private static CodeItem FindCodeItem(IEnumerable<CodeItem> codeItems, CodeItem codeItem)
        {
            foreach (var item in codeItems)
            {
                if (item.Id == codeItem.Id)
                {
                    return item;
                }

                if (item is IMembers hasMembersItem && hasMembersItem.Members.Any())
                {
                    var found = FindCodeItem(hasMembersItem.Members, codeItem);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Set selected/highlighted item within a depth group
        /// </summary>
        /// <remarks>Used for the CodeNav top margin</remarks>
        /// <param name="items">List of code items</param>
        public static void SetSelectedIndex(List<CodeItem> items)
            => items
                .Cast<CodeDepthGroupItem>()
                .ToList()
                .ForEach(groupItem =>
                {
                    var selectedItem = groupItem.Members.LastOrDefault(i => i.IsHighlighted);
                    groupItem.SelectedIndex = selectedItem != null ? groupItem.Members.IndexOf(selectedItem) : 0;
                });

        /// <summary>
        /// Get background highlight color from settings
        /// </summary>
        /// <remarks>
        /// Should be used separate from the actual highlighting to avoid,
        /// reading settings everytime we highlight
        /// </remarks>
        /// <returns>Background color</returns>
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

        public static async Task<T> FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                return null;
            }

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
    }
}
