using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.CodeAnalysis.Text;
using CodeNav.Helpers;
using CodeNav.Windows;
using System;
using System.Runtime.Serialization;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell;

namespace CodeNav.Models
{
    [DataContract]
    public class CodeItem : ObservableObject
    {
        public CodeItem()
        {
            _clickItemCommand = new DelegateCommand(ClickItem, null);
            _goToDefinitionCommand = new DelegateCommand(GoToDefinition, null);
            _goToEndCommand = new DelegateCommand(GoToEnd, null);
            _selectInCodeCommand = new DelegateCommand(SelectInCode, null);
            _copyNameCommand = new DelegateCommand(CopyName, null);
            _refreshCommand = new DelegateCommand(RefreshCodeNav, null);
            _expandAllCommand = new DelegateCommand(ExpandAll, null);
            _collapseAllCommand = new DelegateCommand(CollapseAll, null);
            _bookmarkCommand = new DelegateCommand(Bookmark, null);
            _deleteBookmarkCommand = new DelegateCommand(DeleteBookmark, null);
            _clearBookmarksCommand = new DelegateCommand(ClearBookmarks, null);
            _filterBookmarksCommand = new DelegateCommand(FilterBookmarks, null);
            _customizeBookmarkStylesCommand = new DelegateCommand(CustomizeBookmarkStyles, null);
            _clearHistoryCommand = new DelegateCommand(ClearHistory, null);
        }

        public string Name { get; set; }
        public LinePosition StartLinePosition { get; set; }
        public LinePosition EndLinePosition { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public TextSpan Span { get; set; }
        public ImageMoniker Moniker { get; set; }
        public ImageMoniker OverlayMoniker { get; set; }
        [DataMember]
        public string Id { get; set; }
        public string Tooltip { get; set; }
        internal string FullName;
        public CodeItemKindEnum Kind;
        public CodeItemAccessEnum Access;
        internal ICodeViewUserControl Control;
        public bool IsHighlighted;

        private double _opacity;
        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        #region Status Image
        private ImageMoniker _statusMoniker;
        public ImageMoniker StatusMoniker
        {
            get => _statusMoniker;
            set => SetProperty(ref _statusMoniker, value);
        }

        private Visibility _statusMonikerVisibility = Visibility.Collapsed;
        public Visibility StatusMonikerVisibility
        {
            get => _statusMonikerVisibility;
            set => SetProperty(ref _statusMonikerVisibility, value);
        }

        private bool _statusGrayscale;
        public bool StatusGrayscale
        {
            get => _statusGrayscale;
            set => SetProperty(ref _statusGrayscale, value);
        }

        private double _statusOpacity;
        public double StatusOpacity
        {
            get => _statusOpacity;
            set => SetProperty(ref _statusOpacity, value);
        }
        #endregion

        public List<BookmarkStyle> BookmarkStyles
            => Control.CodeDocumentViewModel.BookmarkStyles;

        public bool FilterOnBookmarks
        {
            get => Control.CodeDocumentViewModel.FilterOnBookmarks;
            set => Control.CodeDocumentViewModel.FilterOnBookmarks = value;
        }

        public bool BookmarksAvailable => Control.CodeDocumentViewModel.Bookmarks.Any();

        private bool _contextMenuIsOpen;
        public bool ContextMenuIsOpen
        {
            get => _contextMenuIsOpen;
            set => SetProperty(ref _contextMenuIsOpen, value);
        }

        #region Fonts
        private float _fontSize;
        public float FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        private float _parameterFontSize;
        public float ParameterFontSize
        {
            get => _parameterFontSize;
            set => SetProperty(ref _parameterFontSize, value);
        }

        private FontFamily _fontFamily;
        public FontFamily FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        private FontStyle _fontStyle;
        public FontStyle FontStyle
        {
            get => _fontStyle;
            set => SetProperty(ref _fontStyle, value);
        }

        private FontWeight _fontWeight;
        public FontWeight FontWeight
        {
            get => _fontWeight;
            set => SetProperty(ref _fontWeight, value);
        }
        #endregion

        #region IsVisible
        private Visibility _visibility;
        public Visibility IsVisible
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }
        #endregion

        #region Foreground
        private Color _foregroundColor;
        public Color ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                SetProperty(ref _foregroundColor, value);
                NotifyPropertyChanged("ForegroundBrush");
            }
        }

        public SolidColorBrush ForegroundBrush => ColorHelper.ToBrush(_foregroundColor);
        #endregion

        #region Background
        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                SetProperty(ref _backgroundColor, value);
                NotifyPropertyChanged("BackgroundBrush");
            }
        }

        public SolidColorBrush BackgroundBrush => ColorHelper.ToBrush(_backgroundColor);

        private Color _nameBackgroundColor;
        public Color NameBackgroundColor
        {
            get => _nameBackgroundColor;
            set
            {
                SetProperty(ref _nameBackgroundColor, value);
                NotifyPropertyChanged("NameBackgroundBrush");
            }
        }

        public SolidColorBrush NameBackgroundBrush => ColorHelper.ToBrush(_nameBackgroundColor);
        #endregion

        #region Commands
        private readonly DelegateCommand _clickItemCommand;
        public ICommand ClickItemCommand => _clickItemCommand;
        public void ClickItem(object args)
        {
            HistoryHelper.AddItemToHistory(this);
            DocumentHelper.ScrollToLine(StartLinePosition).FireAndForget();
        }

        private readonly DelegateCommand _goToDefinitionCommand;
        public ICommand GoToDefinitionCommand => _goToDefinitionCommand;
        public void GoToDefinition(object args) => DocumentHelper.ScrollToLine(StartLinePosition).FireAndForget();

        private readonly DelegateCommand _clearHistoryCommand;
        public ICommand ClearHistoryCommand => _clearHistoryCommand;
        public void ClearHistory(object args) => HistoryHelper.ClearHistory(this);

        private readonly DelegateCommand _goToEndCommand;
        public ICommand GoToEndCommand => _goToEndCommand;
        public void GoToEnd(object args) => DocumentHelper.ScrollToLine(EndLinePosition).FireAndForget();

        private readonly DelegateCommand _selectInCodeCommand;
        public ICommand SelectInCodeCommand => _selectInCodeCommand;
        public void SelectInCode(object args) => DocumentHelper.SelectLines(Span).FireAndForget();

        private readonly DelegateCommand _copyNameCommand;
        public ICommand CopyNameCommand => _copyNameCommand;
        public void CopyName(object args) => Clipboard.SetText(Name);

        private readonly DelegateCommand _refreshCommand;
        public ICommand RefreshCommand => _refreshCommand;
        public void RefreshCodeNav(object args) => Control.UpdateDocument();

        private readonly DelegateCommand _expandAllCommand;
        public ICommand ExpandAllCommand => _expandAllCommand;
        public void ExpandAll(object args) => Control.ToggleAll(true, new List<CodeItem>() { this });

        private readonly DelegateCommand _collapseAllCommand;
        public ICommand CollapseAllCommand => _collapseAllCommand;
        public void CollapseAll(object args) => Control.ToggleAll(false, new List<CodeItem>() { this });

        /// <summary>
        /// Add a single bookmark
        /// </summary>
        private readonly DelegateCommand _bookmarkCommand;
        public ICommand BookmarkCommand => _bookmarkCommand;
        public void Bookmark(object args) => BookmarkAsync(args).FireAndForget();

        public async Task BookmarkAsync(object args)
        {
            try
            {
                var bookmarkStyle = args as BookmarkStyle;

                BookmarkHelper.ApplyBookmark(this, bookmarkStyle);

                var bookmarkStyleIndex = await BookmarkHelper.GetIndex(Control.CodeDocumentViewModel, bookmarkStyle);

                Control.CodeDocumentViewModel.AddBookmark(Id, bookmarkStyleIndex);

                await SaveToSolutionStorage();

                ContextMenuIsOpen = false;

                NotifyPropertyChanged("BookmarksAvailable");
            }
            catch (Exception e)
            {
                LogHelper.Log("CodeItem.Bookmark", e);
            }
        }

        /// <summary>
        /// Delete a single bookmark
        /// </summary>
        private readonly DelegateCommand _deleteBookmarkCommand;
        public ICommand DeleteBookmarkCommand => _deleteBookmarkCommand;
        public void DeleteBookmark(object args)
        {
            try
            {
                BookmarkHelper.ClearBookmark(this);

                Control.CodeDocumentViewModel.RemoveBookmark(Id);

                SaveToSolutionStorage().FireAndForget();

                NotifyPropertyChanged("BookmarksAvailable");
            }
            catch (Exception e)
            {
                LogHelper.Log("CodeItem.DeleteBookmark", e);
            }
        }

        /// <summary>
        /// Clear all bookmarks
        /// </summary>
        private readonly DelegateCommand _clearBookmarksCommand;
        public ICommand ClearBookmarksCommand => _clearBookmarksCommand;
        public void ClearBookmarks(object args)
        {
            try
            {
                Control.CodeDocumentViewModel.ClearBookmarks();

                SaveToSolutionStorage().FireAndForget();

                NotifyPropertyChanged("BookmarksAvailable");
            }
            catch (Exception e)
            {
                LogHelper.Log("CodeItem.ClearBookmarks", e);
            }
        }

        private readonly DelegateCommand _filterBookmarksCommand;
        public ICommand FilterBookmarksCommand => _filterBookmarksCommand;
        public void FilterBookmarks(object args) => Control.FilterBookmarks();

        private readonly DelegateCommand _customizeBookmarkStylesCommand;
        public ICommand CustomizeBookmarkStylesCommand => _customizeBookmarkStylesCommand;
        public void CustomizeBookmarkStyles(object args)
        {
            new CustomizeBookmarkStylesWindow(Control.CodeDocumentViewModel).ShowDialog();
            BookmarkHelper.ApplyBookmarks(Control.CodeDocumentViewModel).FireAndForget();
        }
        #endregion

        private async Task SaveToSolutionStorage()
        {
            var solutionStorageModel = await SolutionStorageHelper.Load<SolutionStorageModel>().ConfigureAwait(false);

            if (solutionStorageModel.Documents == null)
            {
                solutionStorageModel.Documents = new List<CodeDocumentViewModel>();
            }

            var storageItem = solutionStorageModel.Documents
                .FirstOrDefault(d => d.FilePath.Equals(Control.CodeDocumentViewModel.FilePath));
            solutionStorageModel.Documents.Remove(storageItem);

            solutionStorageModel.Documents.Add(Control.CodeDocumentViewModel);

            await SolutionStorageHelper.Save(solutionStorageModel).ConfigureAwait(false);
        }
    }

    public class CodeItemComparer : IEqualityComparer<CodeItem>
    {
        public bool Equals(CodeItem x, CodeItem y)
        {
            //Check whether the objects are the same object. 
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            //Check whether the products' properties are equal. 
            var membersAreEqual = true;
            if (x is CodeClassItem && y is CodeClassItem)
            {
                membersAreEqual = (x as CodeClassItem).Members.SequenceEqual((y as CodeClassItem).Members, new CodeItemComparer());
            }
            if (x is CodeNamespaceItem && y is CodeNamespaceItem)
            {
                membersAreEqual = (x as CodeNamespaceItem).Members.SequenceEqual((y as CodeNamespaceItem).Members, new CodeItemComparer());
            }

            return x != null && y != null && x.Id.Equals(y.Id) && membersAreEqual;
        }

        // Not used, but must be implemented because of interface
        public int GetHashCode(CodeItem obj)
        {
            return 0;
        }
    }
}
