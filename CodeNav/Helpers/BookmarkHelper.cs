using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

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
            foreach (var bookmark in codeDocumentViewModel.Bookmarks)
            {
                var codeItem = codeDocumentViewModel.CodeDocument
                    .Flatten()
                    .First(i => i.Id.Equals(bookmark.Key));
                ApplyBookmark(codeItem, bookmark.Value as BookmarkStyle);
            }
        }

        /// <summary>
        /// Revert all code items to previous styling and delete all bookmarks
        /// </summary>
        /// <param name="codeDocumentViewModel">view model</param>
        public static void ClearBookmarks(CodeDocumentViewModel codeDocumentViewModel)
        {
            foreach (var bookmark in codeDocumentViewModel.Bookmarks)
            {
                var codeItem = codeDocumentViewModel.CodeDocument
                    .Flatten()
                    .First(i => i.Id.Equals(bookmark.Key));
                ClearBookmark(codeItem);
            }

            codeDocumentViewModel.Bookmarks.Clear();
        }

        /// <summary>
        /// Apply bookmark style foreground and background to code item
        /// </summary>
        /// <param name="codeItem">code item</param>
        /// <param name="bookmarkStyle">bookmark style</param>
        public static void ApplyBookmark(CodeItem codeItem, BookmarkStyle bookmarkStyle)
        {
            codeItem.Background = bookmarkStyle.Background;
            codeItem.Foreground = bookmarkStyle.Foreground;
        }

        /// <summary>
        /// Revert code item foreground and background to previous state
        /// </summary>
        /// <param name="codeItem">code item</param>
        public static void ClearBookmark(CodeItem codeItem)
        {
            codeItem.Background = Brushes.Transparent;
            codeItem.Foreground = BrushHelper.ToBrush(EnvironmentColors.ToolWindowTextColorKey);
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
        public static bool IsBookmark(Dictionary<string, BookmarkStyle> bookmarks, CodeItem codeItem)
            => bookmarks.ContainsKey(codeItem.Id);

        /// <summary>
        /// Default available bookmark styles
        /// </summary>
        /// <returns>List of bookmark styles</returns>
        public static List<BookmarkStyle> GetBookmarkStyles() 
            => new List<BookmarkStyle>
            {
                new BookmarkStyle(Brushes.LightYellow, Brushes.Black),
                new BookmarkStyle(Brushes.PaleVioletRed, Brushes.White),
                new BookmarkStyle(Brushes.LightGreen, Brushes.Black),
                new BookmarkStyle(Brushes.LightBlue, Brushes.Black),
                new BookmarkStyle(Brushes.MediumPurple, Brushes.White),
                new BookmarkStyle(Brushes.LightGray, Brushes.Black),
            };
    }
}
