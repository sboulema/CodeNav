using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void ApplyBookmarks(CodeDocumentViewModel codeDocumentViewModel, string solutionFilePath)
        {
            try
            {
                if (!codeDocumentViewModel.CodeDocument.Any()) return;

                _ = GetBookmarkStyles(codeDocumentViewModel, solutionFilePath);

                foreach (var bookmark in codeDocumentViewModel.Bookmarks)
                {
                    var codeItem = codeDocumentViewModel.CodeDocument
                        .Flatten()
                        .FirstOrDefault(i => i.Id.Equals(bookmark.Key));
                    if (codeItem == null) continue;
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
            if (codeItem == null) return;

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
        public static List<BookmarkStyle> GetBookmarkStyles(CodeDocumentViewModel codeDocumentViewModel, string solutionFilePath)
        {
            if (string.IsNullOrEmpty(solutionFilePath)) return GetDefaultBookmarkStyles();

            var solutionStorage = SolutionStorageHelper.Load<SolutionStorageModel>(solutionFilePath);

            if (solutionStorage?.Documents == null) return GetDefaultBookmarkStyles();

            var storageItem = solutionStorage.Documents
                .FirstOrDefault(s => s.FilePath.Equals(codeDocumentViewModel.FilePath));
            if (storageItem != null && storageItem.BookmarkStyles != null && 
                storageItem.BookmarkStyles.Any(bs => bs.BackgroundColor.A != 0))
            {
                codeDocumentViewModel.BookmarkStyles = storageItem.BookmarkStyles;
            }

            if (codeDocumentViewModel.BookmarkStyles == null)
            {
                codeDocumentViewModel.BookmarkStyles = GetDefaultBookmarkStyles();
            }

            return codeDocumentViewModel.BookmarkStyles;
        }

        public static void SetBookmarkStyles(CodeDocumentViewModel codeDocumentViewModel, ControlCollection controls, string solutionFilePath)
        {
            var styles = new List<BookmarkStyle>();

            foreach (var item in controls)
            {
                var label = item as Label;
                styles.Add(new BookmarkStyle(ColorHelper.ToMediaColor(label.BackColor), ColorHelper.ToMediaColor(label.ForeColor)));
            }

            codeDocumentViewModel.BookmarkStyles = styles;   

            SolutionStorageHelper.SaveToSolutionStorage(solutionFilePath, codeDocumentViewModel);
        }

        public static int GetIndex(List<BookmarkStyle> bookmarkStyles, BookmarkStyle bookmarkStyle)
        {
            return bookmarkStyles.FindIndex(b => b.BackgroundColor == bookmarkStyle.BackgroundColor && 
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
    }
}
