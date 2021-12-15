using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using static System.Windows.Forms.Control;

namespace CodeNav.Helpers
{
    public static class BookmarkHelper
    {
        /// <summary>
        /// Apply bookmark style to all code items that are bookmarked
        /// </summary>
        /// <param name="codeDocumentViewModel"></param>
        public static void ApplyBookmarks(CodeDocumentViewModel codeDocumentViewModel)
        {
            try
            {
                if (!codeDocumentViewModel.CodeDocument.Any())
                {
                    return;
                }

                foreach (var bookmark in codeDocumentViewModel.Bookmarks)
                {
                    var codeItem = codeDocumentViewModel.CodeDocument
                        .Flatten()
                        .FirstOrDefault(i => i.Id.Equals(bookmark.Key));

                    if (codeItem == null)
                    {
                        continue;
                    }

                    ApplyBookmark(codeItem, codeDocumentViewModel.BookmarkStyles[bookmark.Value]);
                }
            }
            catch (Exception e)
            {
                LogHelper.Log("ApplyBookmarks", e);
            }
        }

        /// <summary>
        /// Revert all code items to previous styling and delete all bookmarks
        /// </summary>
        /// <param name="codeDocumentViewModel">view model</param>
        public static void ClearBookmarks(CodeDocumentViewModel codeDocumentViewModel)
        {
            try
            {
                foreach (var bookmark in codeDocumentViewModel.Bookmarks)
                {
                    var codeItem = codeDocumentViewModel.CodeDocument
                        .Flatten()
                        .FirstOrDefault(i => i.Id.Equals(bookmark.Key));
                    ClearBookmark(codeItem);
                }

                codeDocumentViewModel.Bookmarks.Clear();
            }
            catch (Exception e)
            {
                LogHelper.Log("ClearBookmarks", e);
            }
        }

        /// <summary>
        /// Apply bookmark style foreground and background to code item
        /// </summary>
        /// <param name="codeItem">code item</param>
        /// <param name="bookmarkStyle">bookmark style</param>
        public static void ApplyBookmark(CodeItem codeItem, BookmarkStyle bookmarkStyle)
        {
            codeItem.BackgroundColor = bookmarkStyle.BackgroundColor;
            codeItem.ForegroundColor = bookmarkStyle.ForegroundColor;
        }

        /// <summary>
        /// Revert code item foreground and background to previous state
        /// </summary>
        /// <param name="codeItem">code item</param>
        public static void ClearBookmark(CodeItem codeItem)
        {
            if (codeItem == null)
            {
                return;
            }

            codeItem.BackgroundColor = Brushes.Transparent.Color;
            codeItem.ForegroundColor = ColorHelper.ToMediaColor(EnvironmentColors.ToolWindowTextColorKey);
        }

        /// <summary>
        /// Is a code item bookmarked
        /// </summary>
        /// <param name="codeDocumentViewModel">view model</param>
        /// <param name="codeItem">code item</param>
        /// <returns>if code item is bookmarked</returns>
        public static bool IsBookmark(CodeDocumentViewModel codeDocumentViewModel, CodeItem codeItem)
            => codeDocumentViewModel.Bookmarks.ContainsKey(codeItem.Id);

        /// <summary>
        /// Is a code item bookmarked
        /// </summary>
        /// <param name="bookmarks">List of bookmarks</param>
        /// <param name="codeItem">code item</param>
        /// <returns>if code item is bookmarked</returns>
        public static bool IsBookmark(Dictionary<string, int> bookmarks, CodeItem codeItem)
            => bookmarks.ContainsKey(codeItem.Id);

        /// <summary>
        /// Default available bookmark styles
        /// </summary>
        /// <returns>List of bookmark styles</returns>
        public static async Task<List<BookmarkStyle>> GetBookmarkStyles(CodeDocumentViewModel codeDocumentViewModel)
        {
            var storageItem = await SolutionStorageHelper.GetStorageItem(codeDocumentViewModel.FilePath);

            if (storageItem == null)
            {
                return GetDefaultBookmarkStyles();
            }

            if (storageItem.BookmarkStyles?.Any(style => style.BackgroundColor.A != 0) == true)
            {
                codeDocumentViewModel.BookmarkStyles = storageItem.BookmarkStyles;
            }

            if (codeDocumentViewModel.BookmarkStyles?.Any() != true)
            {
                codeDocumentViewModel.BookmarkStyles = GetDefaultBookmarkStyles();
            }

            return codeDocumentViewModel.BookmarkStyles;
        }

        public static async Task SetBookmarkStyles(CodeDocumentViewModel codeDocumentViewModel, ControlCollection controls)
        {
            var styles = new List<BookmarkStyle>();

            foreach (var item in controls)
            {
                var label = item as Label;
                styles.Add(new BookmarkStyle(ColorHelper.ToMediaColor(label.BackColor), ColorHelper.ToMediaColor(label.ForeColor)));
            }

            codeDocumentViewModel.BookmarkStyles = styles;

            await SolutionStorageHelper.SaveToSolutionStorage(codeDocumentViewModel);
        }

        public static async Task SetBookmarkStyles(CodeDocumentViewModel codeDocumentViewModel, List<BookmarkStyle> bookmarkStyles)
        {
            codeDocumentViewModel.BookmarkStyles = bookmarkStyles;

            await SolutionStorageHelper.SaveToSolutionStorage(codeDocumentViewModel);
        }

        public static int GetIndex(CodeDocumentViewModel codeDocumentViewModel, BookmarkStyle bookmarkStyle)
        {
            return codeDocumentViewModel.BookmarkStyles.FindIndex(b =>
                b.BackgroundColor == bookmarkStyle.BackgroundColor &&
                b.ForegroundColor == bookmarkStyle.ForegroundColor);
        }

        private static List<BookmarkStyle> GetDefaultBookmarkStyles()
            => new List<BookmarkStyle>
            {
                new BookmarkStyle(Brushes.LightYellow.Color, Brushes.Black.Color),
                new BookmarkStyle(Brushes.PaleVioletRed.Color, Brushes.White.Color),
                new BookmarkStyle(Brushes.LightGreen.Color, Brushes.Black.Color),
                new BookmarkStyle(Brushes.LightBlue.Color, Brushes.Black.Color),
                new BookmarkStyle(Brushes.MediumPurple.Color, Brushes.White.Color),
                new BookmarkStyle(Brushes.LightGray.Color, Brushes.Black.Color),
            };

        public static async Task<Dictionary<string, int>> LoadBookmarksFromStorage(string filePath)
        {
            var storageItem = await SolutionStorageHelper.GetStorageItem(filePath);

            return storageItem?.Bookmarks ?? new Dictionary<string, int>();
        }
    }
}
