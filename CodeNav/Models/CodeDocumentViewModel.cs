using System.Collections.Generic;
using Caliburn.Micro;

namespace CodeNav.Models
{
    public class CodeDocumentViewModel : PropertyChangedBase
    {
        private List<CodeItem> _codeDocument;
        public List<CodeItem> CodeDocument {
            get { return _codeDocument; }
            set
            {
                _codeDocument = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
