using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using CodeNav.Helpers;
using Microsoft.VisualStudio.PlatformUI;

namespace CodeNav.Models
{
    [DataContract]
    public class CodeDocumentViewModel : ObservableObject
    {
        public CodeDocumentViewModel()
        {
            _codeDocument = new List<CodeItem>();
            Bookmarks = new Dictionary<string, int>();
            HistoryItems = new List<CodeItem>();
        }

        private List<CodeItem> _codeDocument;
        public List<CodeItem> CodeDocument {
            get
            {
                return _codeDocument;
            }
            set
            {
                SetProperty(ref _codeDocument, value);
                NotifyPropertyChanged("CodeDocumentTop");
            }
        }

        public List<CodeItem> CodeDocumentTop
        {
            get
            {
                var result = new List<CodeItem>();

                TraverseDepth(_codeDocument, result, 0);

                HighlightHelper.SetSelectedIndex(result);

                return result;
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        private void TraverseDepth(List<CodeItem> items, List<CodeItem> result, int depth)
        {
            foreach (var item in items)
            {
                if (depth >= result.Count)
                {
                    result.Add(new CodeDepthGroupItem());
                }

                (result[depth] as IMembers).Members.Add(item);

                if (item is IMembers hasMembersItem &&
                    hasMembersItem != null &&
                    (hasMembersItem.Members.Any(i => i.IsHighlighted) || item.IsHighlighted))
                {
                    depth++;
                    TraverseDepth(hasMembersItem.Members, result, depth);
                    depth--;
                }
            }
        }

        public bool ShowFilterToolbar => General.Instance.ShowFilterToolbar;

        public Visibility ShowFilterToolbarVisibility => General.Instance.ShowFilterToolbar
            ? Visibility.Visible
            : Visibility.Collapsed;

        public SortOrderEnum SortOrder;

        public Visibility BookmarksAvailable
        {
            get
            {
                return Bookmarks.Any() ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public void AddBookmark(string id, int bookmarkStyleIndex)
        {
            if (Bookmarks.ContainsKey(id))
            {
                Bookmarks.Remove(id);
            }

            Bookmarks.Add(id, bookmarkStyleIndex);

            NotifyPropertyChanged("BookmarksAvailable");
        }

        public void RemoveBookmark(string id)
        {
            Bookmarks.Remove(id);

            NotifyPropertyChanged("BookmarksAvailable");
        }

        public void ClearBookmarks()
        {
            BookmarkHelper.ClearBookmarks(this);

            NotifyPropertyChanged("BookmarksAvailable");
        }

        public Visibility ClearFilterVisibility =>
            string.IsNullOrEmpty(FilterText) ?
            Visibility.Collapsed : Visibility.Visible;

        private string _filterText;
        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                NotifyPropertyChanged("ClearFilterVisibility");
            }
        }

        private Dictionary<string, int> _bookmarks;
        [DataMember]
        public Dictionary<string, int> Bookmarks
        {
            get
            {
                return _bookmarks;
            }
            set
            {
                _bookmarks = value;
                NotifyPropertyChanged("BookmarksAvailable");
            }
        }

        public bool FilterOnBookmarks;

        [DataMember]
        public string FilePath;

        [DataMember]
        public List<BookmarkStyle> BookmarkStyles;

        [DataMember]
        public List<CodeItem> HistoryItems;
    }
}
