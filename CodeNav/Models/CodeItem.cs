using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.CodeAnalysis.Text;
using CodeNav.Helpers;
using CodeNav.Windows;
using System;

namespace CodeNav.Models
{
    public class CodeItem : PropertyChangedBase
    {
        public CodeItem()
        {
            _clickItemCommand = new DelegateCommand(ClickItem);
            _goToDefinitionCommand = new DelegateCommand(GoToDefinition);
            _goToEndCommand = new DelegateCommand(GoToEnd);
            _selectInCodeCommand = new DelegateCommand(SelectInCode);
            _copyNameCommand = new DelegateCommand(CopyName);
            _refreshCommand = new DelegateCommand(Refresh);
            _expandAllRegionsCommand = new DelegateCommand(ExpandAllRegions);
            _collapseAllRegionsCommand = new DelegateCommand(CollapseAllRegions);
            _bookmarkCommand = new DelegateCommand(Bookmark);
            _deleteBookmarkCommand = new DelegateCommand(DeleteBookmark);
            _clearBookmarksCommand = new DelegateCommand(ClearBookmarks);
            _filterBookmarksCommand = new DelegateCommand(FilterBookmarks);
            _customizeBookmarkStylesCommand = new DelegateCommand(CustomizeBookmarkStyles);
        }

        public string Name { get; set; }
        public LinePosition StartLinePosition { get; set; }
        public LinePosition EndLinePosition { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public TextSpan Span { get; set; }
        public ImageMoniker Moniker { get; set; }
        public ImageMoniker OverlayMoniker { get; set; }
        public string Id { get; set; }
        public string Tooltip { get; set; }
        internal string FullName;
        public CodeItemKindEnum Kind;
        public CodeItemAccessEnum Access;
        internal CodeViewUserControl Control;

        private ImageMoniker _statusMoniker;
        public ImageMoniker StatusMoniker
        {
            get
            {
                return _statusMoniker;
            }
            set
            {
                _statusMoniker = value;                
                NotifyOfPropertyChange();
            }
        }

        private Visibility _statusMonikerVisibility = Visibility.Collapsed;
        public Visibility StatusMonikerVisibility
        {
            get
            {
                return _statusMonikerVisibility;
            }
            set
            {
                _statusMonikerVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public List<BookmarkStyle> BookmarkStyles {
            get
            {
                return BookmarkHelper.GetBookmarkStyles(Control.CodeDocumentViewModel, Control.Dte?.Solution?.FileName);
            }
        }

        public bool FilterOnBookmarks
        {
            get
            {
                return Control.CodeDocumentViewModel.FilterOnBookmarks;
            }
            set
            {
                Control.CodeDocumentViewModel.FilterOnBookmarks = value;
            }
        }

        public bool BookmarksAvailable
        {
            get
            {
                return Control.CodeDocumentViewModel.Bookmarks.Any();
            }
        }

        private bool _contextMenuIsOpen;
        public bool ContextMenuIsOpen
        {
            get
            {
                return _contextMenuIsOpen;
            }
            set
            {
                _contextMenuIsOpen = value;
                NotifyOfPropertyChange();
            }
        }

        #region Fonts
        private float _fontSize;
        public float FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value;
                NotifyOfPropertyChange();
            }
        }

        private float _parameterFontSize;
        public float ParameterFontSize
        {
            get
            {
                return _parameterFontSize;
            }
            set
            {
                _parameterFontSize = value;
                NotifyOfPropertyChange();
            }
        }

        private FontFamily _fontFamily;
        public FontFamily FontFamily
        {
            get
            {
                return _fontFamily;
            }
            set
            {
                _fontFamily = value;
                NotifyOfPropertyChange();
            }
        }

        private FontStyle _fontStyle;
        public FontStyle FontStyle
        {
            get
            {
                return _fontStyle;
            }
            set
            {
                _fontStyle = value;
                NotifyOfPropertyChange();
            }
        }

        private FontWeight _fontWeight;
        public FontWeight FontWeight
        {
            get
            {
                return _fontWeight;
            }
            set
            {
                _fontWeight = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region IsVisible
        private Visibility _visibility;
        public Visibility IsVisible
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Foreground
        private SolidColorBrush _foreground;
        public SolidColorBrush Foreground
        {
            get
            {
                return _foreground;
            }
            set
            {
                _foreground = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Background
        private SolidColorBrush _background;
        public SolidColorBrush Background
        {
            get
            {
                return _background;
            }
            set
            {
                _background = value;
                NotifyOfPropertyChange();
            }
        }

        private SolidColorBrush _nameBackground;
        public SolidColorBrush NameBackground
        {
            get
            {
                return _nameBackground;
            }
            set
            {
                _nameBackground = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Commands
        private readonly DelegateCommand _clickItemCommand;
        public ICommand ClickItemCommand => _clickItemCommand;
        public void ClickItem(object startLinePosition) => Control.SelectLine(startLinePosition);

        private readonly DelegateCommand _goToDefinitionCommand;
        public ICommand GoToDefinitionCommand => _goToDefinitionCommand;
        public void GoToDefinition(object args) => Control.SelectLine(StartLinePosition);

        private readonly DelegateCommand _goToEndCommand;
        public ICommand GoToEndCommand => _goToEndCommand;
        public void GoToEnd(object args) => Control.SelectLine(EndLinePosition);

        private readonly DelegateCommand _selectInCodeCommand;
        public ICommand SelectInCodeCommand => _selectInCodeCommand;
        public void SelectInCode(object args) => Control.Select(StartLinePosition, EndLinePosition);

        private readonly DelegateCommand _copyNameCommand;
        public ICommand CopyNameCommand => _copyNameCommand;
        public void CopyName(object args) => Clipboard.SetText(Name);

        private readonly DelegateCommand _refreshCommand;
        public ICommand RefreshCommand => _refreshCommand;
        public void Refresh(object args) => Control.UpdateDocument(true);

        private readonly DelegateCommand _expandAllRegionsCommand;
        public ICommand ExpandAllRegionsCommand => _expandAllRegionsCommand;
        public void ExpandAllRegions(object args) => Control.ToggleAllRegions(true);

        private readonly DelegateCommand _collapseAllRegionsCommand;
        public ICommand CollapseAllRegionsCommand => _collapseAllRegionsCommand;
        public void CollapseAllRegions(object args) => Control.ToggleAllRegions(false);

        /// <summary>
        /// Add a single bookmark
        /// </summary>
        private readonly DelegateCommand _bookmarkCommand;
        public ICommand BookmarkCommand => _bookmarkCommand;
        public void Bookmark(object args)
        {
            try
            {
                var bookmarkStyle = args as BookmarkStyle;

                BookmarkHelper.ApplyBookmark(this, bookmarkStyle);

                Control.CodeDocumentViewModel.AddBookmark(Id, 
                    BookmarkHelper.GetIndex(BookmarkStyles, bookmarkStyle));

                SaveToSolutionStorage();

                ContextMenuIsOpen = false;

                NotifyOfPropertyChange("BookmarksAvailable");              
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

                SaveToSolutionStorage();

                NotifyOfPropertyChange("BookmarksAvailable");
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

                SaveToSolutionStorage();

                NotifyOfPropertyChange("BookmarksAvailable");
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
            var solutionFilePath = Control.Dte?.Solution?.FileName;
            new CustomizeBookmarkStylesWindow(Control.CodeDocumentViewModel, solutionFilePath).ShowDialog();
            BookmarkHelper.ApplyBookmarks(Control.CodeDocumentViewModel, solutionFilePath);
        }
        #endregion

        private void SaveToSolutionStorage()
        {
            if (string.IsNullOrEmpty(Control?.Dte?.Solution?.FileName)) return;

            var solutionStorageModel = SolutionStorageHelper.Load<SolutionStorageModel>(Control.Dte.Solution.FileName);

            if (solutionStorageModel.Documents == null)
            {
                solutionStorageModel.Documents = new List<CodeDocumentViewModel>();
            }

            var storageItem = solutionStorageModel.Documents
                .FirstOrDefault(d => d.FilePath.Equals(Control.CodeDocumentViewModel.FilePath));
            solutionStorageModel.Documents.Remove(storageItem);

            solutionStorageModel.Documents.Add(Control.CodeDocumentViewModel);

            SolutionStorageHelper.Save<SolutionStorageModel>(Control.Dte.Solution.FileName, solutionStorageModel);
        }
    }

    public class CodeItemComparer : IEqualityComparer<CodeItem>
    {
        public bool Equals(CodeItem x, CodeItem y)
        {
            //Check whether the objects are the same object. 
            if (ReferenceEquals(x, y)) return true;

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
