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

        Task UpdateDocument(string filePath = "", bool forceUpdate = false);

        void HighlightCurrentItem();

        void ToggleAll(bool isExpanded, List<CodeItem> root = null);

        void FilterBookmarks();
    }
}
