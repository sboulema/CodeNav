using CodeNav.Models;
using Microsoft.VisualStudio.Text.Outlining;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeNav
{
    public interface ICodeViewUserControl
    {
        CodeDocumentViewModel CodeDocumentViewModel { get; set; }

        void RegionsCollapsed(RegionsCollapsedEventArgs e);

        void RegionsExpanded(RegionsExpandedEventArgs e);

        Task UpdateDocument(bool forceUpdate = false);

        List<CodeItem> CreateLineThresholdPassedItem();

        Task HighlightCurrentItem();

        Task<bool> IsLargeDocument();

        Task SelectLine(object startLinePosition, bool extend = false);

        Task Select(object startLinePosition, object endLinePosition);

        void ToggleAll(bool isExpanded, List<CodeItem> root = null);

        void FilterBookmarks();
    }
}
