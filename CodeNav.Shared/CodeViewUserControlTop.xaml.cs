﻿using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Task = System.Threading.Tasks.Task;

namespace CodeNav
{
    public partial class CodeViewUserControlTop : ICodeViewUserControl
    {
        private readonly RowDefinition _row;

        public CodeDocumentViewModel CodeDocumentViewModel { get; set; }

        public CodeViewUserControlTop(RowDefinition row = null)
        {
            InitializeComponent();

            // Setup viewmodel as datacontext
            CodeDocumentViewModel = new CodeDocumentViewModel();
            DataContext = CodeDocumentViewModel;

            _row = row;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => UpdateDocument();

        public void FilterBookmarks()
            => VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel);

        public void RegionsCollapsed(RegionsCollapsedEventArgs e) =>
            OutliningHelper.RegionsCollapsed(e, CodeDocumentViewModel.CodeDocument);

        public void RegionsExpanded(RegionsExpandedEventArgs e) =>
            OutliningHelper.RegionsExpanded(e, CodeDocumentViewModel.CodeDocument);

        public void ToggleAll(bool isExpanded, List<CodeItem> root = null)
        {
            OutliningHelper.ToggleAll(root ?? CodeDocumentViewModel.CodeDocument, isExpanded);
        }

        public void UpdateDocument(string filePath = "")
            => ThreadHelper.JoinableTaskFactory.RunAsync(async () => await DocumentHelper.UpdateDocument(this, CodeDocumentViewModel,
                null, _row, filePath));

        public void HighlightCurrentItem(CaretPositionChangedEventArgs e, Color backgroundBrushColor)
        {
            HighlightHelper.HighlightCurrentItem(
                CodeDocumentViewModel,
                backgroundBrushColor,
                e.NewPosition.BufferPosition.GetContainingLine().LineNumber);

            // Force NotifyPropertyChanged
            CodeDocumentViewModel.CodeDocumentTop = null;
        }

        public async Task RegisterDocumentEvents()
        {
            // Todo: Implement events
        }
    }
}
