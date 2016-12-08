using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using CodeNav.Properties;

namespace CodeNav.Models
{
    public class CodeDocumentViewModel : PropertyChangedBase
    {
        public List<CodeItem> CodeDocument { get; set; }

        private double _maxWidth;
        public double MaxWidth
        {
            get
            {
                return _maxWidth;
            }
            set
            {
                _maxWidth = value;
                NotifyOfPropertyChange();
            }
        }

        public void LoadMaxWidth()
        {
            MaxWidth = Settings.Default.Width - 10;
        }

        public void LoadCodeDocument(List<CodeItem> codeDocument)
        {
            CodeDocument = codeDocument;
            OnPropertyChanged(new PropertyChangedEventArgs("CodeDocument"));
        }
    }
}
