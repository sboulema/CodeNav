#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CodeNav.Helpers;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Editor;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using CodeNav.Models;
using Microsoft.VisualStudio.Shell;

namespace CodeNav
{
    public class CodeNavMargin : DockPanel, IWpfTextViewMargin
    {
        public const string MarginName = "CodeNav";
        private bool _isDisposed;

        public ICodeViewUserControl? _control;
        public readonly IWpfTextView? _textView;
        private readonly ColumnDefinition? _codeNavColumn;
        private readonly Grid _codeNavGrid;
        public readonly MarginSideEnum MarginSide;

        public CodeNavMargin(IWpfTextViewHost textViewHost, MarginSideEnum side)
        {
            // Wire up references for the event handlers in RegisterEvents
            MarginSide = side;

            // Add the view/content to the margin area
            if (side == MarginSideEnum.Top)
            {
                _codeNavGrid = CreateGridTop(textViewHost);
            }
            else
            {
                _codeNavGrid = CreateGrid(textViewHost);
                _codeNavColumn = _codeNavGrid.ColumnDefinitions[(MarginSideEnum)General.Instance.MarginSide == MarginSideEnum.Left ? 0 : 2];
            }

            Children.Add(_codeNavGrid);
        }

        private Grid CreateGrid(IWpfTextViewHost textViewHost)
        {
            var marginWidth = General.Instance.ShowMargin ? General.Instance.Width : 0;

            var leftColumnWidth = new GridLength(marginWidth, GridUnitType.Pixel);
            if ((MarginSideEnum)General.Instance.MarginSide != MarginSideEnum.Left)
            {
                leftColumnWidth = new GridLength(0, GridUnitType.Star);
            }

            var rightColumnWidth = new GridLength(0, GridUnitType.Star);
            if ((MarginSideEnum)General.Instance.MarginSide != MarginSideEnum.Left)
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

            var columnIndex = (MarginSideEnum)General.Instance.MarginSide == MarginSideEnum.Left ? 0 : 2;

            _control = new CodeViewUserControl(grid.ColumnDefinitions[columnIndex]);

            grid.Children.Add(_control as UIElement);

            Grid.SetColumn(_control as UIElement, columnIndex);
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(textViewHost.HostControl, (MarginSideEnum)General.Instance.MarginSide == MarginSideEnum.Left ? 2 : 0);

            var gridChild = grid.GetGridChildByType<CodeViewUserControl>();

            if (gridChild != null)
            {
                if (General.Instance.BackgroundColor.IsNamedColor && General.Instance.BackgroundColor.Name.Equals("Transparent"))
                {
                    gridChild.Background = Brushes.Transparent;
                }
                else
                {
                    gridChild.Background = ColorHelper.ToBrush(General.Instance.BackgroundColor);
                }
            }

            return grid;
        }

        private Grid CreateGridTop(IWpfTextViewHost textViewHost)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(0, General.Instance.ShowMargin ? GridUnitType.Star : GridUnitType.Pixel)
            });
            grid.RowDefinitions.Add(new RowDefinition());

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            _control = new CodeViewUserControlTop(grid.RowDefinitions[0]);

            Grid.SetRow(_control as UIElement, 0);
            Grid.SetRow(textViewHost.HostControl, 1);

            // Apply custom background color to CodeNav grid child
            (_control as CodeViewUserControlTop)!.Background =
                General.Instance.BackgroundColor.IsNamedColor && General.Instance.BackgroundColor.Name.Equals("Transparent") 
                ? Brushes.Transparent : ColorHelper.ToBrush(General.Instance.BackgroundColor);

            return grid;
        }

        /// <summary>
        /// Remove the margin
        /// </summary>
        public void Remove()
        {
            Children.Remove(_codeNavGrid);
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
            if (_codeNavColumn == null)
            {
                return;
            }

            VisibilityHelper.SetMarginWidth(_codeNavColumn, _codeNavColumn.Width != new GridLength(0)).FireAndForget();
            General.Instance.ShowMargin = !General.Instance.ShowMargin;
            General.Instance.Save();
        }

        private void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!(_control is FrameworkElement controlElement))
            {
                return;
            }

            if (!double.IsNaN(controlElement.ActualWidth) && controlElement.ActualWidth != 0)
            {
                General.Instance.Width = controlElement.ActualWidth;
                General.Instance.Save();
            }
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
        public ITextViewMargin? GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        /// <summary>
        /// Disposes an instance of <see cref="CodeNavMargin"/> class.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed || _control == null)
            {
                return;
            }

            _control.CaretPositionChangedSubscription?.Dispose();
            _control.TextContentChangedSubscription?.Dispose();
            _control.UpdateWhileTypingSubscription?.Dispose();
            _control.FileActionOccurredSubscription?.Dispose();

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
