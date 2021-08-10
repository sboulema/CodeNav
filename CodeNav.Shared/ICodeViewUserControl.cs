using CodeNav.Models;
using Microsoft.VisualStudio.Text.Outlining;
using System.Collections.Generic;

namespace CodeNav
{
    public interface ICodeViewUserControl
    {
        CodeDocumentViewModel CodeDocumentViewModel { get; set; }

        void RegionsCollapsed(RegionsCollapsedEventArgs e);

        void RegionsExpanded(RegionsExpandedEventArgs e);

        void UpdateDocument(string filePath = "");

        void HighlightCurrentItem(int lineNumber);

        void ToggleAll(bool isExpanded, List<CodeItem> root = null);

        void FilterBookmarks();
    }
}
