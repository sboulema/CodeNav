using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;

namespace CodeNav.Models
{
    public class CodeDocumentViewModel : PropertyChangedBase
    {
        public List<CodeItem> CodeDocument { get; set; }

        public void LoadCodeDocument(List<CodeItem> codeDocument)
        {
            CodeDocument = codeDocument;
            OnPropertyChanged(new PropertyChangedEventArgs("CodeDocument"));
        }
    }
}
