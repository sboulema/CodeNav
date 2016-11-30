using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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

        private CodeViewUserControl codeViewUserControl;
        private readonly CodeDocumentViewModel codeDocumentVM;

        public CodeNav(IWpfTextViewHost textViewHost, DTE dte)
        {
            if (dte.ActiveDocument == null) return;

            codeDocumentVM = new CodeDocumentViewModel();           
            var document = new List<CodeItem>();
           
            foreach (CodeElement codeElement in dte.ActiveDocument.ProjectItem.FileCodeModel.CodeElements)
            {
                if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                {
                    foreach (CodeElement namespaceMember in (codeElement as CodeNamespace).Members)
                    {
                        switch (namespaceMember.Kind)
                        {
                            case vsCMElement.vsCMElementClass:
                                document.Add(CodeItemMapper.MapClass(namespaceMember));
                                break;
                            case vsCMElement.vsCMElementEnum:
                                document.Add(CodeItemMapper.MapEnum(namespaceMember));
                                break;
                            case vsCMElement.vsCMElementInterface:
                                document.Add(CodeItemMapper.MapInterface(namespaceMember));
                                break;
                        }
                    }
                }
            }

            codeDocumentVM.LoadCodeDocument(document);

            Children.Add(CreateGrid(textViewHost, dte));
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

            codeViewUserControl = new CodeViewUserControl(dte) { DataContext = codeDocumentVM };
            codeViewUserControl.BorderThickness = new Thickness(0);
            codeViewUserControl.ClipToBounds = Enabled;
            grid.Children.Add(codeViewUserControl);

            Grid.SetColumn(codeViewUserControl, 0);
            Grid.SetColumn(splitter, 1);
            Grid.SetColumn(textViewHost.HostControl, 2);

            return grid;
        }

        private void LeftDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!double.IsNaN(codeViewUserControl.ActualWidth))
            {
                Settings.Default.Width = codeViewUserControl.ActualWidth;
                Settings.Default.Save();
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
