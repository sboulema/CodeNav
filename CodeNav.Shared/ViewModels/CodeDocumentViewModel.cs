#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using CodeNav.Helpers;
using Microsoft.VisualStudio.PlatformUI;

namespace CodeNav.Models.ViewModels
{
    [DataContract]
    public class CodeDocumentViewModel : ObservableObject
    {
        private List<CodeItem> _codeDocument = new List<CodeItem>();
        public List<CodeItem> CodeDocument
        {
            get => _codeDocument;
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
            set => NotifyPropertyChanged();
        }

        private void TraverseDepth(List<CodeItem> items, List<CodeItem> result, int depth)
        {
            foreach (var item in items)
            {
                if (depth >= result.Count)
                {
                    result.Add(new CodeDepthGroupItem());
                }

                if (!(result[depth] is IMembers memberItem))
                {
                    continue;
                }

                memberItem.Members.Add(item);

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
            => Bookmarks.Any() ? Visibility.Visible : Visibility.Collapsed;

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

        private string _filterText = string.Empty;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                NotifyPropertyChanged("ClearFilterVisibility");
            }
        }

        private Dictionary<string, int> _bookmarks = new Dictionary<string, int>();
        [DataMember]
        public Dictionary<string, int> Bookmarks
        {
            get => _bookmarks;
            set
            {
                _bookmarks = value;
                NotifyPropertyChanged("BookmarksAvailable");
            }
        }

        public bool FilterOnBookmarks;

        [DataMember]
        public string FilePath = string.Empty;

        [DataMember]
        public List<BookmarkStyle> BookmarkStyles = new List<BookmarkStyle>();

        [DataMember]
        public SynchronizedCollection<CodeItem> HistoryItems = new SynchronizedCollection<CodeItem>();
    }
}
