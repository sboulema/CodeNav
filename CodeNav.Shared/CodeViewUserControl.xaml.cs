using System.Collections.Generic;
using System.Windows.Controls;
using CodeNav.Helpers;
using CodeNav.Models;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Outlining;
using Task = System.Threading.Tasks.Task;

namespace CodeNav
{
    /// <summary>
    /// Interaction logic for CodeViewUserControl.xaml
    /// </summary>
    public partial class CodeViewUserControl : ICodeViewUserControl
    {
        private readonly ColumnDefinition _column;
        private List<CodeItem> _cache;
        public CodeDocumentViewModel CodeDocumentViewModel { get; set; }
        internal IOutliningManagerService OutliningManagerService;
        private VisualStudioWorkspace _workspace;
        private readonly CodeNavMargin _margin;

        public CodeViewUserControl(ColumnDefinition column = null,
            IOutliningManagerService outliningManagerService = null,
            VisualStudioWorkspace workspace = null, CodeNavMargin margin = null)
        {
            InitializeComponent();

            // Setup viewmodel as datacontext
            CodeDocumentViewModel = new CodeDocumentViewModel();
            DataContext = CodeDocumentViewModel;

            _column = column;
            OutliningManagerService = outliningManagerService;
            _workspace = workspace;
            _margin = margin;

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public void SetWorkspace(VisualStudioWorkspace workspace) => _workspace = workspace;

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e) => _ = UpdateDocument(forceUpdate: true);

        public void FilterBookmarks()
            => VisibilityHelper.SetCodeItemVisibility(CodeDocumentViewModel);

        public void RegionsCollapsed(RegionsCollapsedEventArgs e) =>
            OutliningHelper.RegionsCollapsed(e, CodeDocumentViewModel.CodeDocument);

        public void RegionsExpanded(RegionsExpandedEventArgs e) =>
            OutliningHelper.RegionsExpanded(e, CodeDocumentViewModel.CodeDocument);

        public void ToggleAll(bool isExpanded, List<CodeItem> root = null)
            => OutliningHelper.ToggleAll(root ?? CodeDocumentViewModel.CodeDocument, isExpanded);

        public async Task UpdateDocument(string filePath = "", bool forceUpdate = false)
            => await DocumentHelper.UpdateDocument(this, _workspace, CodeDocumentViewModel, _cache,
                OutliningManagerService, _margin, _column, null, filePath, forceUpdate).ConfigureAwait(false);

        #region Custom Items

        /// <summary>
        /// Show an item to indicate that the user has to select an active code document to inspect
        /// </summary>
        public void ShowWaitingForDocument()
        {
            CodeDocumentViewModel.CodeDocument = PlaceholderHelper.CreateSelectDocumentItem();
        }

        #endregion

        public void HighlightCurrentItem()
            => _ = HighlightHelper.HighlightCurrentItem(CodeDocumentViewModel);
    }
}
