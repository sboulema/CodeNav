using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;

namespace CodeNav.Shared.ViewModels
{
    public class BookmarkStylesWindowViewModel : ObservableObject
    {
        private List<BookmarkStyle> _bookmarkStyles = new List<BookmarkStyle>();

        public List<BookmarkStyle> BookmarkStyles
        {
            get => _bookmarkStyles;
            set => SetProperty(ref _bookmarkStyles, value);
        }

        private BookmarkStyle _selectedBookmarkStyle;

        public BookmarkStyle SelectedBookmarkStyle
        {
            get => _selectedBookmarkStyle;
            set => SetProperty(ref _selectedBookmarkStyle, value);
        }
    }
}