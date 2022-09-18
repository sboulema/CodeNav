#nullable enable

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
        public string Name { get; set; } = string.Empty;

        public LinePosition? StartLinePosition { get; set; }

        public LinePosition? EndLinePosition { get; set; }

        public int? StartLine { get; set; }

        public int? EndLine { get; set; }

        public TextSpan Span { get; set; }

        public ImageMoniker Moniker { get; set; }

        public ImageMoniker OverlayMoniker { get; set; }

        [DataMember]
        public string Id { get; set; } = string.Empty;

        public string Tooltip { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        internal string FullName = string.Empty;

        public CodeItemKindEnum Kind;

        public CodeItemAccessEnum Access;

        internal ICodeViewUserControl? Control;

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
            => Control?.CodeDocumentViewModel.BookmarkStyles ?? new List<BookmarkStyle>();

        public bool FilterOnBookmarks
        {
            get => Control?.CodeDocumentViewModel.FilterOnBookmarks ?? false;
            set => Control!.CodeDocumentViewModel.FilterOnBookmarks = value;
        }

        public bool BookmarksAvailable => Control?.CodeDocumentViewModel.Bookmarks.Any() == true;

        private bool _contextMenuIsOpen;
        public bool ContextMenuIsOpen
        {
            get => _contextMenuIsOpen;
            set => SetProperty(ref _contextMenuIsOpen, value);
        }

        public bool IsDoubleClicked;

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

        private FontFamily? _fontFamily;
        public FontFamily? FontFamily
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
        public ICommand ClickItemCommand => new DelegateCommand(ClickItem);
        public void ClickItem() => ClickItemAsync().FireAndForget();

        private async Task ClickItemAsync()
        {
            await Task.Delay(200);

            if (IsDoubleClicked)
            {
                IsDoubleClicked = false;
                return;
            }

            HistoryHelper.AddItemToHistory(this);
            await DocumentHelper.ScrollToLine(StartLinePosition, FilePath);
        }

        public ICommand GoToDefinitionCommand => new DelegateCommand(GoToDefinition);
        public void GoToDefinition(object args) => DocumentHelper.ScrollToLine(StartLinePosition).FireAndForget();

        public ICommand ClearHistoryCommand => new DelegateCommand(ClearHistory);
        public void ClearHistory(object args) => HistoryHelper.ClearHistory(this);

        public ICommand GoToEndCommand => new DelegateCommand(GoToEnd);
        public void GoToEnd(object args) => DocumentHelper.ScrollToLine(EndLinePosition).FireAndForget();

        public ICommand SelectInCodeCommand => new DelegateCommand(SelectInCode);
        public void SelectInCode(object args) => DocumentHelper.SelectLines(Span).FireAndForget();

        public ICommand EditLineCommand => new DelegateCommand(EditLine);
        public void EditLine(object args)
        {
            IsDoubleClicked = true;
            DocumentHelper.EditLine(StartLinePosition, FilePath).FireAndForget();
        }

        public ICommand CopyNameCommand => new DelegateCommand(CopyName);
        public void CopyName(object args) => Clipboard.SetText(Name);

        public ICommand RefreshCommand => new DelegateCommand(RefreshCodeNav);
        public void RefreshCodeNav(object args) => Control?.UpdateDocument();

        public ICommand ExpandAllCommand => new DelegateCommand(ExpandAll);
        public void ExpandAll(object args) => Control?.ToggleAll(true, new List<CodeItem>() { this });

        public ICommand CollapseAllCommand => new DelegateCommand(CollapseAll);
        public void CollapseAll(object args) => Control?.ToggleAll(false, new List<CodeItem>() { this });

        /// <summary>
        /// Add a single bookmark
        /// </summary>
        public ICommand BookmarkCommand => new DelegateCommand(Bookmark);
        public void Bookmark(object args) => BookmarkAsync(args).FireAndForget();

        public async Task BookmarkAsync(object args)
        {
            try
            {
                var bookmarkStyle = args as BookmarkStyle;

                if (bookmarkStyle == null ||
                    Control?.CodeDocumentViewModel == null)
                {
                    return;
                }

                BookmarkHelper.ApplyBookmark(this, bookmarkStyle);

                var bookmarkStyleIndex = BookmarkHelper.GetIndex(Control.CodeDocumentViewModel, bookmarkStyle);

                Control.CodeDocumentViewModel.AddBookmark(Id, bookmarkStyleIndex);

                await SolutionStorageHelper.SaveToSolutionStorage(Control.CodeDocumentViewModel);

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
        public ICommand DeleteBookmarkCommand => new DelegateCommand(DeleteBookmark);
        public void DeleteBookmark(object args)
        {
            try
            {
                BookmarkHelper.ClearBookmark(this);

                Control?.CodeDocumentViewModel.RemoveBookmark(Id);

                SolutionStorageHelper.SaveToSolutionStorage(Control?.CodeDocumentViewModel).FireAndForget();

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
        public ICommand ClearBookmarksCommand => new DelegateCommand(ClearBookmarks);
        public void ClearBookmarks(object args)
        {
            try
            {
                Control?.CodeDocumentViewModel.ClearBookmarks();

                SolutionStorageHelper.SaveToSolutionStorage(Control?.CodeDocumentViewModel).FireAndForget();

                NotifyPropertyChanged("BookmarksAvailable");
            }
            catch (Exception e)
            {
                LogHelper.Log("CodeItem.ClearBookmarks", e);
            }
        }

        public ICommand FilterBookmarksCommand => new DelegateCommand(FilterBookmarks);
        public void FilterBookmarks(object args) => Control?.FilterBookmarks();

        public ICommand CustomizeBookmarkStylesCommand => new DelegateCommand(CustomizeBookmarkStyles);
        public void CustomizeBookmarkStyles(object args)
        {
            if (Control?.CodeDocumentViewModel == null)
            {
                return;
            }

            new BookmarkStylesWindow(Control.CodeDocumentViewModel).ShowDialog();
            BookmarkHelper.ApplyBookmarks(Control.CodeDocumentViewModel);
        }
        #endregion
    }
}
