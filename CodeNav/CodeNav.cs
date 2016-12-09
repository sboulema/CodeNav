using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Mappers;
using CodeNav.Models;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.VisualStudio.Text.Editor;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Window = EnvDTE.Window;

namespace CodeNav
{
    internal class CodeNav : DockPanel, IWpfTextViewMargin
    {
        public const string MarginName = "CodeNav";
        private bool _isDisposed;

        private CodeViewUserControl _codeViewUserControl;
        private readonly CodeDocumentViewModel _codeDocumentVm;
        private readonly DTE _dte;
        private readonly IWpfTextView _textView;
        private readonly DocumentEvents _documentEvents;
        private List<string> _highlightedItems;

        public CodeNav(IWpfTextViewHost textViewHost, DTE dte)
        {
            if (dte.ActiveDocument?.ProjectItem?.FileCodeModel?.CodeElements == null) return;

            _highlightedItems = new List<string>();

            _dte = dte;
            _textView = textViewHost.TextView;
            _documentEvents = dte.Events.DocumentEvents;

            _codeDocumentVm = new CodeDocumentViewModel();
            _codeDocumentVm.LoadMaxWidth();

            Children.Add(CreateGrid(textViewHost, dte));

            var windowEvents = dte.Events.WindowEvents;
            windowEvents.WindowActivated += WindowEvents_WindowActivated;          
        }

        public void RegisterEvents()
        {
            if (_textView?.Caret != null)
            {
                _textView.Caret.PositionChanged += Caret_PositionChanged;
            }             
            _documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;
        }

        public void UnRegisterEvents()
        {
            _textView.Caret.PositionChanged -= Caret_PositionChanged;
            _documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;
        }

        private void DocumentEvents_DocumentSaved(Document document) => UpdateDocument();
        private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus) => UpdateDocument();
        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) => UpdateCurrentItem();

        private void UpdateCurrentItem()
        {
            if (_dte.ActiveDocument?.Selection == null || _codeDocumentVm?.CodeDocument == null) return;

            var textSelection = _dte.ActiveDocument.Selection as TextSelection;

            var currentFunctionElement = textSelection?.ActivePoint.CodeElement[vsCMElement.vsCMElementFunction];

            if (currentFunctionElement == null)
            {
                UnHighlight(_codeDocumentVm.CodeDocument, _highlightedItems);
                return;
            }

            UnHighlight(_codeDocumentVm.CodeDocument, _highlightedItems);

            _highlightedItems = new List<string>();
            GetItemsToHighlight(_highlightedItems, currentFunctionElement);

            Highlight(_codeDocumentVm.CodeDocument, _highlightedItems);         
        }

        private static void GetItemsToHighlight(List<string> list, CodeElement element)
        {
            list.Add(element.FullName);

            try
            {
                var parent = element.Collection.Parent;
                GetItemsToHighlight(list, parent);
            }
            catch (Exception)
            {
            }
        }

        private static void UnHighlight(List<CodeItem> document, List<string> itemNames)
        {
            foreach (var name in itemNames)
            {
                var item = FindCodeItem(document, name);
                if (item == null) return;

                item.Foreground = new SolidColorBrush(Colors.Black);

                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).BorderBrush = new SolidColorBrush(Colors.DarkGray);
                }
            }
        }

        private static void Highlight(List<CodeItem> document, List<string> itemNames)
        {
            foreach (var name in itemNames)
            {
                var item = FindCodeItem(document, name);
                if (item == null) return;

                item.Foreground = new SolidColorBrush(Colors.SteelBlue);

                if (item is CodeClassItem)
                {
                    (item as CodeClassItem).BorderBrush = new SolidColorBrush(Colors.SteelBlue);
                }
            }
        }

        private static CodeItem FindCodeItem(List<CodeItem> items, string itemFullName)
        {
            foreach (var item in items)
            {
                if (item.FullName.Equals(itemFullName))
                {
                    return item;
                }

                if (item is CodeClassItem)
                {
                    var classItem = (CodeClassItem)item;
                    if (classItem.Members.Any())
                    {
                        var found = FindCodeItem(classItem.Members, itemFullName);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
            }
            return null;
        }

        private void UpdateDocument()
        {
            var elements = _dte.ActiveDocument?.ProjectItem?.FileCodeModel?.CodeElements;
            if (elements == null) return;

            var document = CodeItemMapper.MapDocument(elements);
            _codeDocumentVm.LoadCodeDocument(document);
            _codeDocumentVm.LoadMaxWidth();
        }

        private Grid CreateGrid(IWpfTextViewHost textViewHost, DTE dte)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Settings.Default.Width, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition());

            var splitter = new GridSplitter
            {
                Width = 5,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            splitter.DragCompleted += LeftDragCompleted;
            grid.Children.Add(splitter);

            _codeViewUserControl = new CodeViewUserControl(dte) { DataContext = _codeDocumentVm };
            grid.Children.Add(_codeViewUserControl);

            Grid.SetColumn(_codeViewUserControl, 0);
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(textViewHost.HostControl, 2);

            return grid;
        }

        private void LeftDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!Double.IsNaN(_codeViewUserControl.ActualWidth))
            {
                Settings.Default.Width = _codeViewUserControl.ActualWidth;
                Settings.Default.Save();
                _codeDocumentVm.LoadMaxWidth();
            }
        }

        #region IWpfTextViewMargin

        /// <summary>
        /// Gets the <see cref="FrameworkElement"/> that implements the visual representation of the margin.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin

        /// <summary>
        /// Gets the size of the margin.
        /// </summary>
        /// <remarks>
        /// For a horizontal margin this is the height of the margin,
        /// since the width will be determined by the <see cref="ITextView"/>.
        /// For a vertical margin this is the width of the margin,
        /// since the height will be determined by the <see cref="ITextView"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();

                // Since this is a horizontal margin, its width will be bound to the width of the text view.
                // Therefore, its size is its height.
                return ActualHeight;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the margin is enabled.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public bool Enabled
        {
            get
            {
                ThrowIfDisposed();

                // The margin should always be enabled
                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="ITextViewMargin"/> with the given <paramref name="marginName"/> or null if no match is found
        /// </summary>
        /// <param name="marginName">The name of the <see cref="ITextViewMargin"/></param>
        /// <returns>The <see cref="ITextViewMargin"/> named <paramref name="marginName"/>, or null if no match is found.</returns>
        /// <remarks>
        /// A margin returns itself if it is passed its own name. If the name does not match and it is a container margin, it
        /// forwards the call to its children. Margin name comparisons are case-insensitive.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="marginName"/> is null.</exception>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return String.Equals(marginName, MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        /// <summary>
        /// Disposes an instance of <see cref="CodeNav"/> class.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            UnRegisterEvents();
            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

        #endregion

            /// <summary>
            /// Checks and throws <see cref="ObjectDisposedException"/> if the object is disposed.
            /// </summary>
        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(MarginName);
            }
        }
    }
}
