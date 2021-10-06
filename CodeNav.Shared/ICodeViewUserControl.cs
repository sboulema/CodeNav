using CodeNav.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeNav
{
    public interface ICodeViewUserControl
    {
        CodeDocumentViewModel CodeDocumentViewModel { get; set; }

        void UpdateDocument(string filePath = "");

        void HighlightCurrentItem(int lineNumber);

        void ToggleAll(bool isExpanded, List<CodeItem> root = null);

        void FilterBookmarks();

        Task RegisterDocumentEvents();
    }
}
