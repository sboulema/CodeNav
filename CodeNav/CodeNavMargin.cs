using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Properties;
using EnvDTE;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Window = EnvDTE.Window;
using CodeNav.Models;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace CodeNav
{
    public class CodeNavMargin : DockPanel, IWpfTextViewMargin
    {
        public const string MarginName = "CodeNav";
        private bool _isDisposed;

        public ICodeViewUserControl _control;      
        private readonly DTE _dte;
        public readonly IWpfTextView _textView;      
        private readonly Window _window;
        private readonly ColumnDefinition _codeNavColumn;
        private readonly Grid _codeNavGrid;
        private WindowEvents _windowEvents;
        private DocumentEvents _documentEvents;
        private readonly IOutliningManagerService _outliningManagerService;
        private readonly IOutliningManager _outliningManager;
        private readonly VisualStudioWorkspace _workspace;
        public readonly MarginSideEnum MarginSide;

        public CodeNavMargin(IWpfTextViewHost textViewHost, DTE dte, IOutliningManagerService outliningManagerService, 
            VisualStudioWorkspace workspace, MarginSideEnum side)
        {
            // Wire up references for the event handlers in RegisterEvents
            _dte = dte;
            _textView = textViewHost.TextView;
            _window = GetWindow(textViewHost, dte);
            _outliningManagerService = outliningManagerService;
            _outliningManager = OutliningHelper.GetOutliningManager(outliningManagerService, _textView);
            _workspace = workspace;
            MarginSide = side;

            // If we can not find the window we belong to we can not do anything
            if (_window == null) return;

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

            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            RegisterEvents();      

            UpdateSettings();
        }

        /// <summary>
        /// Get window belonging to textViewHost
        /// </summary>
        /// <param name="textViewHost"></param>
        /// <param name="dte"></param>
        /// <returns></returns>
        private static Window GetWindow(IWpfTextViewHost textViewHost, _DTE dte)
        {
            if (textViewHost == null || dte == null) return null;

            textViewHost.TextView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument document);

            return GetWindows(dte).FirstOrDefault(w => 
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
                return w?.Document?.FullName?.Equals(document.FilePath, StringComparison.InvariantCultureIgnoreCase) == true;
            });
        }

        /// <summary>
        /// Get all open windows as list
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        private static List<Window> GetWindows(_DTE dte)
        {
            var windowsList = new List<Window>();

            try
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

                for (var i = 1; i < dte.Windows.Count + 1; i++)
                {
                    windowsList.Add(dte.Windows.Item(i));
                }
            }
            catch (COMException)
            {
                // Unspecified error (Exception from HRESULT: 0x80004005 (E_FAIL)) 
            }
            catch (Exception e)
            {
                LogHelper.Log("Exception getting parent window", e);
            }

            return windowsList;
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

            _control = new CodeViewUserControl(_window, grid.ColumnDefinitions[columnIndex],
                textViewHost.TextView, _outliningManagerService, _workspace, this, _dte);

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

            _control = new CodeViewUserControlTop(_window, grid.RowDefinitions[0],
                textViewHost.TextView, _outliningManagerService, _workspace, this, _dte);

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
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();

            // Subscribe to Cursor move event
            if (_textView?.Caret != null)
            {
                _textView.Caret.PositionChanged -= Caret_PositionChanged;
                _textView.Caret.PositionChanged += Caret_PositionChanged;
            }

            // Subscribe to TextBuffer changes
            if (_textView.TextBuffer != null && Settings.Default.ShowHistoryIndicators)
            {
                _textView.TextBuffer.ChangedLowPriority -= TextBuffer_ChangedLowPriority;
                _textView.TextBuffer.ChangedLowPriority += TextBuffer_ChangedLowPriority;
            }

            // Subscribe to Document Save event
            if (_window != null)
            {  
                _documentEvents = _dte.Events.DocumentEvents[_window.Document];
                _documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;
                _documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;
            }     

            // Subscribe to Code window activated event
            if (_window != null)
            {
                _windowEvents = _dte.Events.WindowEvents[_window];
                _windowEvents.WindowActivated -= WindowEvents_WindowActivated;
                _windowEvents.WindowActivated += WindowEvents_WindowActivated;
            }

            // Subscribe to Outlining events
            if (_outliningManager != null)
            {
                _outliningManager.RegionsExpanded -= OutliningManager_RegionsExpanded;
                _outliningManager.RegionsExpanded += OutliningManager_RegionsExpanded;
                _outliningManager.RegionsCollapsed -= OutliningManager_RegionsCollapsed;
                _outliningManager.RegionsCollapsed += OutliningManager_RegionsCollapsed;
            }
        }

        private void TextBuffer_ChangedLowPriority(object sender, TextContentChangedEventArgs e)
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
            _textView.TextBuffer.ChangedLowPriority -= TextBuffer_ChangedLowPriority;

            if (_documentEvents != null)
            {
                _documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;
            }

            if (_windowEvents != null)
            {
                _windowEvents.WindowActivated -= WindowEvents_WindowActivated;
            }        
        }

        #pragma warning disable VSTHRD100
        private async void DocumentEvents_DocumentSaved(Document document)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!_control.IsLargeDocument())
            {
                await _control.UpdateDocumentAsync();
            }
            else
            {
                _control.CodeDocumentViewModel.CodeDocument = _control.CreateLineThresholdPassedItem();
            }
        }

        private async void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!_control.IsLargeDocument())
            {
                await _control.UpdateDocumentAsync();
            }
            else
            {
                _control.CodeDocumentViewModel.CodeDocument = _control.CreateLineThresholdPassedItem();
            }
        }
#pragma warning restore VSTHRD100

        private void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.VerifyAccess();
            _control.HighlightCurrentItem();
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
            if (_isDisposed) return;

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

        #endregion
    }
}
