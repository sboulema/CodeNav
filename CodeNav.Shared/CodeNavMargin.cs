using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Helpers;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using CodeNav.Models;
using System.Linq;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using Settings = CodeNav.Properties.Settings;
using Microsoft.VisualStudio.Shell;

namespace CodeNav
{
    public class CodeNavMargin : DockPanel, IWpfTextViewMargin
    {
        public const string MarginName = "CodeNav";
        private bool _isDisposed;

        public ICodeViewUserControl _control;
        public readonly IWpfTextView _textView;
        private readonly ColumnDefinition _codeNavColumn;
        private readonly Grid _codeNavGrid;
        private readonly IOutliningManagerService _outliningManagerService;
        private readonly IOutliningManager _outliningManager;
        private readonly VisualStudioWorkspace _workspace;
        public readonly MarginSideEnum MarginSide;

        public CodeNavMargin(IWpfTextViewHost textViewHost, IOutliningManagerService outliningManagerService,
            VisualStudioWorkspace workspace, MarginSideEnum side)
        {
            // Wire up references for the event handlers in RegisterEvents
            _textView = textViewHost.TextView;
            _outliningManagerService = outliningManagerService;
            _outliningManager = OutliningHelper.GetOutliningManager(outliningManagerService, _textView);
            _workspace = workspace;
            MarginSide = side;

            // Add the view/content to the margin area
            if (side == MarginSideEnum.Top)
            {
                _codeNavGrid = CreateGridTop(textViewHost);
            }
            else
            {
                _codeNavGrid = CreateGrid(textViewHost);
                _codeNavColumn = _codeNavGrid.ColumnDefinitions[Settings.Default.MarginSide == MarginSideEnum.Left ? 0 : 2];
            }

            Children.Add(_codeNavGrid);

            RegisterEvents();

            UpdateSettings();
        }

        private Grid CreateGrid(IWpfTextViewHost textViewHost)
        {
            var marginWidth = Settings.Default.ShowMargin ? Settings.Default.Width : 0;

            var leftColumnWidth = new GridLength(marginWidth, GridUnitType.Pixel);
            if (Settings.Default.MarginSide != MarginSideEnum.Left)
            {
                leftColumnWidth = new GridLength(0, GridUnitType.Star);
            }

            var rightColumnWidth = new GridLength(0, GridUnitType.Star);
            if (Settings.Default.MarginSide != MarginSideEnum.Left)
            {
                rightColumnWidth = new GridLength(marginWidth, GridUnitType.Pixel);
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
                Background = ColorHelper.ToBrush(EnvironmentColors.EnvironmentBackgroundColorKey),
                ToolTip = "What you can do with this bar:" + Environment.NewLine +
                "- double-click it to toggle CodeNav visibility" + Environment.NewLine +
                "- click and drag it to adjust CodeNav width"
            };
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
            splitter.DragCompleted += DragCompleted;
            splitter.MouseDoubleClick += Splitter_MouseDoubleClick;
            grid.Children.Add(splitter);

            var columnIndex = Settings.Default.MarginSide == MarginSideEnum.Left ? 0 : 2;

            _control = new CodeViewUserControl(grid.ColumnDefinitions[columnIndex], _outliningManagerService, _workspace, this);

            grid.Children.Add(_control as UIElement);

            Grid.SetColumn(_control as UIElement, columnIndex);
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(textViewHost.HostControl, Settings.Default.MarginSide == MarginSideEnum.Left ? 2 : 0);

            if (Settings.Default.WindowBackgroundColor.IsNamedColor && Settings.Default.WindowBackgroundColor.Name.Equals("Transparent"))
            {
                grid.GetGridChildByType<CodeViewUserControl>().Background = Brushes.Transparent;
            }
            else
            {
                grid.GetGridChildByType<CodeViewUserControl>().Background = ColorHelper.ToBrush(Settings.Default.WindowBackgroundColor);
            }

            return grid;
        }

        private Grid CreateGridTop(IWpfTextViewHost textViewHost)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(0, Settings.Default.ShowMargin ? GridUnitType.Star : GridUnitType.Pixel)
            });
            grid.RowDefinitions.Add(new RowDefinition());

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            _control = new CodeViewUserControlTop(grid.RowDefinitions[0], _outliningManagerService, _workspace, this);

            Grid.SetRow(_control as UIElement, 0);
            Grid.SetRow(textViewHost.HostControl, 1);

            // Apply custom background color to CodeNav grid child
            (_control as CodeViewUserControlTop).Background = 
                Settings.Default.WindowBackgroundColor.IsNamedColor && Settings.Default.WindowBackgroundColor.Name.Equals("Transparent") 
                ? Brushes.Transparent : ColorHelper.ToBrush(Settings.Default.WindowBackgroundColor);

            return grid;
        }

        /// <summary>
        /// Remove the margin
        /// </summary>
        public void Remove()
        {
            Children.Remove(_codeNavGrid);
        }

        /// <summary>
        /// Copy user settings from previous application version if necessary
        /// </summary>
        private void UpdateSettings()
        {
            if (Settings.Default.NewVersionInstalled)
            {
                Settings.Default.Upgrade();
                Settings.Default.NewVersionInstalled = false;
                Settings.Default.Save();
            }
        }

        #region Events

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            if (_control is CodeViewUserControl)
            {
                ((GridSplitter)_codeNavGrid.Children[0]).Background = ColorHelper.ToBrush(EnvironmentColors.EnvironmentBackgroundColorKey);              
            }
        }

        private void Splitter_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VisibilityHelper.SetMarginWidth(_codeNavColumn, _codeNavColumn.Width != new GridLength(0));
            Settings.Default.ShowMargin = !Settings.Default.ShowMargin;
            Settings.Default.Save();
        }

        private void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!double.IsNaN((_control as FrameworkElement).ActualWidth) && (_control as FrameworkElement).ActualWidth != 0)
            {
                Settings.Default.Width = (_control as FrameworkElement).ActualWidth;
                Settings.Default.Save();
            }
        }

        public void RegisterEvents()
        {
            // Subscribe to Cursor move event
            if (_textView?.Caret != null &&
                !Settings.Default.DisableHighlight)
            {
                _textView.Caret.PositionChanged -= Caret_PositionChanged;
                _textView.Caret.PositionChanged += Caret_PositionChanged;
            }

            // Subscribe to TextBuffer changes
            if ((_textView.TextBuffer as ITextBuffer2) != null &&
                Settings.Default.ShowHistoryIndicators)
            {
                var textBuffer2 = _textView.TextBuffer as ITextBuffer2;

                textBuffer2.ChangedOnBackground -= TextBuffer_ChangedOnBackground;
                textBuffer2.ChangedOnBackground += TextBuffer_ChangedOnBackground;
            }

            // Subscribe to Document events
            VS.Events.DocumentEvents.Saved += DocumentEvents_Saved;

            // Subscribe to Outlining events
            if (_outliningManager != null)
            {
                _outliningManager.RegionsExpanded -= OutliningManager_RegionsExpanded;
                _outliningManager.RegionsExpanded += OutliningManager_RegionsExpanded;
                _outliningManager.RegionsCollapsed -= OutliningManager_RegionsCollapsed;
                _outliningManager.RegionsCollapsed += OutliningManager_RegionsCollapsed;
            }

            VS.Events.WindowEvents.ActiveFrameChanged -= WindowEvents_ActiveFrameChanged;
            VS.Events.WindowEvents.ActiveFrameChanged += WindowEvents_ActiveFrameChanged;
        }

        private void WindowEvents_ActiveFrameChanged(ActiveFrameChangeEventArgs obj)
            => WindowChangedEvent(obj).FireAndForget();

        private void DocumentEvents_Saved(object sender, string e)
            => UpdateDocument().FireAndForget();

        private async Task WindowChangedEvent(ActiveFrameChangeEventArgs obj)
        {
            if (obj.OldFrame == obj.NewFrame)
            {
                return;
            }

            var documentView = await obj.NewFrame.GetDocumentViewAsync();

            var filePath = documentView?.Document?.FilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            UpdateDocument(filePath).FireAndForget();
        }

        private void TextBuffer_ChangedOnBackground(object sender, TextContentChangedEventArgs e)
        {
            var changedSpans = e.Changes.Select(c => c.OldSpan);

            foreach (var span in changedSpans)
            {
                HistoryHelper.AddItemToHistory(_control.CodeDocumentViewModel, span);
            }
        }

        private void OutliningManager_RegionsCollapsed(object sender, RegionsCollapsedEventArgs e) =>
            _control.RegionsCollapsed(e);

        private void OutliningManager_RegionsExpanded(object sender, RegionsExpandedEventArgs e) =>
            _control.RegionsExpanded(e);

        public void UnRegisterEvents()
        {
            _textView.Caret.PositionChanged -= Caret_PositionChanged;

            if ((_textView.TextBuffer as ITextBuffer2) != null)
            {
                (_textView.TextBuffer as ITextBuffer2).ChangedOnBackground -= TextBuffer_ChangedOnBackground;
            }
        }

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            var oldLineNumber = e.OldPosition.BufferPosition.GetContainingLine().LineNumber;
            var newLineNumber = e.NewPosition.BufferPosition.GetContainingLine().LineNumber;

            if (oldLineNumber == newLineNumber)
            {
                return;
            }

            _control.HighlightCurrentItem(newLineNumber);
        }

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
            if (_isDisposed)
            {
                return;
            }

            UnRegisterEvents();

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

        private async Task UpdateDocument(string filePath = "")
        {
            if (!await DocumentHelper.IsLargeDocument().ConfigureAwait(false))
            {
                _control.UpdateDocument(filePath);
            }
            else
            {
                _control.CodeDocumentViewModel.CodeDocument = PlaceholderHelper.CreateLineThresholdPassedItem();
            }
        }

        #endregion
    }
}
