using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Window = EnvDTE.Window;

namespace CodeNav
{
    public class CodeNavMargin : DockPanel, IWpfTextViewMargin
    {
        public const string MarginName = "CodeNav";
        private bool _isDisposed;

        private CodeViewUserControl _control;      
        private readonly DTE _dte;
        private readonly IWpfTextView _textView;      
        private readonly Window _window;
        private readonly ColumnDefinition _codeNavColumn;
        private readonly Grid _codeNavGrid;
        private WindowEvents _windowEvents;
        private DocumentEvents _documentEvents;

        public CodeNavMargin(IWpfTextViewHost textViewHost, DTE dte)
        {
            // Wire up references for the event handlers in RegisterEvents
            _dte = dte;
            _textView = textViewHost.TextView;
            _window = GetWindow(textViewHost, dte);

            // If we can not find the window we belong to we can not do anything
            if (_window == null) return;

            // Add the view/content to the margin area
            _codeNavGrid = CreateGrid(textViewHost, dte);
            _codeNavColumn = _codeNavGrid.ColumnDefinitions[Settings.Default.MarginSide.Equals("Left") ? 0 : 2];
            Children.Add(_codeNavGrid);        

            RegisterEvents();

            LogHelper.Log($"CodeNav initialized for {_window.Caption}");
        }

        /// <summary>
        /// Get window belonging to textViewHost
        /// </summary>
        /// <param name="textViewHost"></param>
        /// <param name="dte"></param>
        /// <returns></returns>
        private static Window GetWindow(IWpfTextViewHost textViewHost, _DTE dte)
        {            
            ITextDocument document;
            textViewHost.TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document);

            for (var i = 1; i < dte.Windows.Count + 1; i++)
            {
                var window = dte.Windows.Item(i);
                try
                {
                    if (window.Document == null) continue;
                    if (!window.Document.FullName.Equals(document.FilePath, StringComparison.InvariantCultureIgnoreCase)) continue;
                    return window;
                }
                catch (Exception e)
                {
                    LogHelper.Log($"Exception getting parent window: {e.Message}");
                }
            }

            return null;
        }

        private Grid CreateGrid(IWpfTextViewHost textViewHost, DTE dte)
        {
            var leftColumnWidth = new GridLength(Settings.Default.Width, GridUnitType.Pixel);
            if (!Settings.Default.MarginSide.Equals("Left"))
            {
                leftColumnWidth = new GridLength(0, GridUnitType.Star);
            }

            var rightColumnWidth = new GridLength(0, GridUnitType.Star);
            if (!Settings.Default.MarginSide.Equals("Left"))
            {
                rightColumnWidth = new GridLength(Settings.Default.Width, GridUnitType.Pixel);
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = leftColumnWidth });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = rightColumnWidth });
            grid.RowDefinitions.Add(new RowDefinition());

            var splitter = new GridSplitter
            {
                Width = 5,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = ToBrush(EnvironmentColors.EnvironmentBackgroundColorKey),
                ToolTip = "What you can do with this bar:" + Environment.NewLine +
                "- double-click it to toggle CodeNav visibility" + Environment.NewLine +
                "- click and drag it to adjust CodeNav width"
            };
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
            splitter.DragCompleted += DragCompleted;
            splitter.MouseDoubleClick += Splitter_MouseDoubleClick;
            grid.Children.Add(splitter);

            var columnIndex = Settings.Default.MarginSide.Equals("Left") ? 0 : 2;

            _control = new CodeViewUserControl(_window, grid.ColumnDefinitions[columnIndex]);
            grid.Children.Add(_control);

            Grid.SetColumn(_control, columnIndex);
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(textViewHost.HostControl, Settings.Default.MarginSide.Equals("Left") ? 2 : 0);

            return grid;
        }

        private static SolidColorBrush ToBrush(ThemeResourceKey key)
        {
            var color = VSColorTheme.GetThemedColor(key);
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        #region Events

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            ((GridSplitter)_codeNavGrid.Children[0]).Background =
                ToBrush(EnvironmentColors.EnvironmentBackgroundColorKey);
        }

        private void Splitter_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => 
            VisibilityHelper.SetMarginWidth(_codeNavColumn, _codeNavColumn.Width != new GridLength(0));

        private void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!double.IsNaN(_control.ActualWidth) && _control.ActualWidth != 0)
            {
                Settings.Default.Width = _control.ActualWidth;
                Settings.Default.Save();
            }
        }

        public void RegisterEvents()
        {
            // Subscribe to Cursor move event
            if (_textView?.Caret != null)
            {
                _textView.Caret.PositionChanged -= Caret_PositionChanged;
                _textView.Caret.PositionChanged += Caret_PositionChanged;
            }

            // Subscribe to Document Save event
            if (_window == null) return;
            _documentEvents = _dte.Events.DocumentEvents[_window.Document];
            _documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;
            _documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

            // Subscribe to Code window activated event
            if (_window == null) return;
            _windowEvents = _dte.Events.WindowEvents[_window];
            _windowEvents.WindowActivated -= WindowEvents_WindowActivated;
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;
        }

        public void UnRegisterEvents()
        {
            _textView.Caret.PositionChanged -= Caret_PositionChanged;
            _documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;

            if (_windowEvents != null)
            {
                _windowEvents.WindowActivated -= WindowEvents_WindowActivated;
            }        
        }

        private void DocumentEvents_DocumentSaved(Document document) => _control.UpdateDocument();
        private void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus) => _control.UpdateDocument();
        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e) => _control.HighlightCurrentItem();

        #endregion

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

                // Since this is a vertical margin, its height will be bound to the height of the text view.
                // Therefore, its size is its width.
                return ActualWidth;
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
        /// Disposes an instance of <see cref="CodeNavMargin"/> class.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            UnRegisterEvents();

            _control?.Dispose();

            GC.SuppressFinalize(this);
            _isDisposed = true;
        }

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

        #endregion
    }
}
