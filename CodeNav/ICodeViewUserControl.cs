using CodeNav.Models;
using EnvDTE;
using Microsoft.VisualStudio.Text.Outlining;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CodeNav
{
    public interface ICodeViewUserControl
    {
        CodeDocumentViewModel CodeDocumentViewModel { get; set; }

        DTE Dte { get; set; }

        void RegionsCollapsed(RegionsCollapsedEventArgs e);

        void RegionsExpanded(RegionsExpandedEventArgs e);

        Task UpdateDocumentAsync(bool forceUpdate = false);

        List<CodeItem> CreateLineThresholdPassedItem();

        void HighlightCurrentItem();

        bool IsLargeDocument();

        void SelectLine(object startLinePosition, bool extend = false);

        void Select(object startLinePosition, object endLinePosition);

        void ToggleAll(bool isExpanded, List<CodeItem> root = null);

        void FilterBookmarks();
    }
}
