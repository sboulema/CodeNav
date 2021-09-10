using System.Collections.Generic;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Outlining;

namespace CodeNav
{
    /// <summary>
    /// Interaction logic for CodeViewUserControl.xaml
    /// </summary>
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
            => DocumentHelper.UpdateDocument(this, CodeDocumentViewModel,
                null, _row, filePath).FireAndForget();

        public void HighlightCurrentItem(int lineNumber)
        {
            HighlightHelper.HighlightCurrentItem(CodeDocumentViewModel, lineNumber).FireAndForget();

            // Force NotifyPropertyChanged
            CodeDocumentViewModel.CodeDocumentTop = null;
        }
    }
}
