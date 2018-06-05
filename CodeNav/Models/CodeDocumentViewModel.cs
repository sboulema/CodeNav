using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using Caliburn.Micro;
using CodeNav.Properties;

namespace CodeNav.Models
{
    [DataContract]
    public class CodeDocumentViewModel : PropertyChangedBase
    {
        public CodeDocumentViewModel()
        {
            _codeDocument = new List<CodeItem>();
            Bookmarks = new Dictionary<string, BookmarkStyle>();
        }

        private List<CodeItem> _codeDocument;
        public List<CodeItem> CodeDocument {
            get
            {
                return _codeDocument;
            }
            set
            {
                _codeDocument = value;
                NotifyOfPropertyChange();
            }
        }

        public bool ShowFilterToolbar => Settings.Default.ShowFilterToolbar;

        public Visibility ShowFilterToolbarVisibility => Settings.Default.ShowFilterToolbar
            ? Visibility.Visible
            : Visibility.Collapsed;

        public SortOrderEnum SortOrder;

        [DataMember]
        public Dictionary<string, BookmarkStyle> Bookmarks;

        public bool FilterOnBookmarks;

        [DataMember]
        public string FilePath;
    }
}
