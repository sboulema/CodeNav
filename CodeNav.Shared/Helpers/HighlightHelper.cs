using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CodeNav.Extensions;
using CodeNav.Models;
using CodeNav.Models.ViewModels;
using Microsoft.VisualStudio.PlatformUI;

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
                Highlight(codeDocumentViewModel, lineNumber, backgroundColor);
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
        private static void Highlight(CodeDocumentViewModel codeDocumentViewModel,
            int lineNumber, Color backgroundColor)
        {
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
            }
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
    }
}
