using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Mappers;
using CodeNav.Models;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.VisualStudio.Text.Editor;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

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

        public CodeNav(IWpfTextViewHost textViewHost, DTE dte)
        {
            if (dte.ActiveDocument?.ProjectItem?.FileCodeModel?.CodeElements == null) return;

            _dte = dte;
            _textView = textViewHost.TextView;
            _documentEvents = dte.Events.DocumentEvents;

            _codeDocumentVm = new CodeDocumentViewModel();

            var document = CodeItemMapper.MapDocument(dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements);

            _codeDocumentVm.LoadCodeDocument(document);
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
        private void WindowEvents_WindowActivated(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus) => UpdateDocument();
        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) => UpdateCurrentItem();

        private void UpdateCurrentItem()
        {
            if (_dte.ActiveDocument?.Selection == null || _codeDocumentVm?.CodeDocument == null) return;

            var textSelection = _dte.ActiveDocument.Selection as TextSelection;

            var currentFunctionElement = textSelection?.ActivePoint.CodeElement[vsCMElement.vsCMElementFunction];
            var currentClassElement = textSelection?.ActivePoint.CodeElement[vsCMElement.vsCMElementClass] ??
                                      textSelection?.ActivePoint.CodeElement[vsCMElement.vsCMElementInterface];

            if (currentFunctionElement == null) return;

            CodeItem currentCodeItem = null;
            CodeClassItem currentClassItem = null;
            foreach (var classItem in _codeDocumentVm.CodeDocument)
            {
                classItem.Foreground = new SolidColorBrush(Colors.Black);
                ((CodeClassItem)classItem).BorderBrush = new SolidColorBrush(Colors.DarkGray);
                if (classItem.Id.Equals(currentClassElement.Name))
                {
                    currentClassItem = (CodeClassItem)classItem;
                }

                foreach (var functionItem in ((CodeClassItem)classItem).Members)
                {
                    functionItem.Foreground = new SolidColorBrush(Colors.Black);
                    if (functionItem.Id.Equals(currentFunctionElement.Name))
                    {
                        currentCodeItem = functionItem;
                    }
                }
            }

            if (currentCodeItem != null)
            {
                currentCodeItem.Foreground = new SolidColorBrush(Colors.SteelBlue);
            }

            if (currentClassItem != null)
            {
                currentClassItem.BorderBrush = currentClassItem.Foreground = new SolidColorBrush(Colors.SteelBlue);                
            }               
        }

        private void UpdateDocument()
        {
            var elements = _dte.ActiveDocument?.ProjectItem?.FileCodeModel?.CodeElements;
            if (elements == null) return;

            var document = CodeItemMapper.MapDocument(elements);
            _codeDocumentVm.LoadCodeDocument(document);          
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

        private void LeftDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!double.IsNaN(_codeViewUserControl.ActualWidth))
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
            return string.Equals(marginName, MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
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
